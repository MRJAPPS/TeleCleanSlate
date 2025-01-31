//MRJ
using TdLib;
using TeleCleanSlate.Common;

namespace TeleCleanSlate.Telegram.Security;

/// <summary>
/// This class manages the authentication process.
/// </summary>
internal class AuthorizationHandler : IDisposable
{
    #region FIELDS
    #region PRIVATE
    #region CALLBACKS
    private readonly Func<AuthorizationHandler, string> OnNeedCode, OnNeedPassword;
    private readonly Action<AuthorizationHandler>? OnReadyToUse;
    private readonly Action<AuthorizationHandler, Exception>? OnUnknownError;
    private readonly Action<AuthorizationHandler, TdException>? OnError;
    #endregion
    private readonly TdClient client;
    private readonly string dbLocation;
    private readonly string appVersion;
    private readonly string data;
    private readonly string apiHash;
    private readonly int apiId;
    private readonly string deviceName;
    private readonly string languageCode;
    private readonly LoginMethod loginMethod;
    private readonly ManualResetEventSlim Ready = new();
    private readonly bool tryAgain;
    #endregion
    #endregion

    #region PROPERTYS
    #region PUBLIC
    /// <summary>
    /// Gets the <see cref="TdClient"/> instance used for communication with the Telegram API.
    /// </summary>
    public TdClient Client => client;
    /// <summary>
    /// Gets the file path where the database is stored.
    /// </summary>
    public string DbLocation => dbLocation;
    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    public string AppVersion => appVersion;
    /// <summary>
    /// Gets the phone number or email address used for authentication, depending on the <see cref="LoginType"/>.
    /// </summary>
    public string Data => data;
    /// <summary>
    /// Gets the name of the device where the application is running.
    /// </summary>
    public string DeviceName => deviceName;
    /// <summary>
    /// Gets the code of the language used for the application (e.g., "en" for English).
    /// </summary>
    public string LanguageCode => languageCode;
    /// <summary>
    /// Gets the login method used for authentication (<see cref="LoginMethod.PhoneNumber"/> or <see cref="LoginMethod.Email"/>).
    /// </summary>
    public LoginMethod LoginType => loginMethod;
    #endregion
    #endregion

    #region TYPES
    #region PUBLIC
    /// <summary>
    /// Specifies the method used for user authentication.
    /// </summary>
    public enum LoginMethod
    {
        /// <summary>
        /// Authentication using a phone number.
        /// </summary>
        PhoneNumber,

        /// <summary>
        /// Authentication using an email address.
        /// </summary>
        Email,
    }
    #endregion
    #endregion

    #region METHODS
    #region PRIVATE
    /// <summary>
    /// Handles the user login by checking the authentication code provided by the user.
    /// The code handling varies based on the login method (<see cref="LoginMethod.PhoneNumber"/> or <see cref="LoginMethod.Email"/>).
    /// </summary>
    /// <param name="code">The authentication code provided by the user.</param>
    private async Task HandleUserLoginCode(string code)
    {
        switch (loginMethod)
        {
            case LoginMethod.PhoneNumber:
                {
                    await client.CheckAuthenticationCodeAsync(code);
                    break;
                }
            case LoginMethod.Email:
                {
                    await client.CheckAuthenticationEmailCodeAsync(new TdApi.EmailAddressAuthentication.EmailAddressAuthenticationCode()
                    {
                        Code = code
                    });
                    break;
                }
        }
    }
    /// <summary>
    /// Handles the user login by checking the authentication password provided by the user.
    /// </summary>
    /// <param name="pass">The password provided by the user.</param>
    private async Task HandleUserLoginPassword(string pass)
    {
        await client.CheckAuthenticationPasswordAsync(pass);
    }
    /// <summary>
    /// Handles the initial user login process based on the login method (<see cref="LoginMethod.PhoneNumber"/> or <see cref="LoginMethod.Email"/>).
    /// </summary>
    private async Task HandleUserLogin()
    {
        switch (loginMethod)
        {
            case LoginMethod.PhoneNumber:
                {
                    await client.SetAuthenticationPhoneNumberAsync(data);
                    break;
                }
            case LoginMethod.Email:
                {
                    await client.SetAuthenticationEmailAddressAsync(data);
                    break;
                }
        }
    }
    /// <summary>
    /// Manages the transition between different authorization states and executes the appropriate actions.
    /// </summary>
    /// <param name="authorizationState">The current state of authorization.</param>
    private async Task HandleUpdateAuthorizationState(TdApi.AuthorizationState authorizationState)
    {
        switch (authorizationState)
        {
            case TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters:
                await AuthorizationStateWaitTdlibParametersHandler();
                break;
            case TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber:
            case TdApi.AuthorizationState.AuthorizationStateWaitEmailAddress:
                await AuthorizationStateWaitPhoneNumberAndEmailHandler();
                break;
            case TdApi.AuthorizationState.AuthorizationStateWaitCode:
            case TdApi.AuthorizationState.AuthorizationStateWaitEmailCode:
                await AuthorizationStateWaitCodeHandler();
                break;
            case TdApi.AuthorizationState.AuthorizationStateWaitPassword:
                await AuthorizationStateWaitPasswordHandler();
                break;
            case TdApi.AuthorizationState.AuthorizationStateWaitRegistration:
                throw new TdException(new TdApi.Error()
                {
                    Code = CommonConstants.TdErrorCodes.UNREGISTERED,
                    Message = "The user is unregistered!"
                });
            case TdApi.AuthorizationState.AuthorizationStateLoggingOut:
            case TdApi.AuthorizationState.AuthorizationStateClosing:
            case TdApi.AuthorizationState.AuthorizationStateClosed:
                break;
            case TdApi.AuthorizationState.AuthorizationStateReady:
                {
                    Ready.Set();
                    OnReadyToUse?.Invoke(this);
                    break;
                }
        }
    }
    /// <summary>
    /// Handles the state where the user needs to provide a phone number or email address for authentication.
    /// If an error occurs, the operation may be retried based on the tryAgain flag.
    /// </summary>
    private async Task AuthorizationStateWaitPhoneNumberAndEmailHandler()
    {
        bool shouldRetry = true;
        while (shouldRetry)
        {
            try
            {
                await HandleUserLogin();
                break;
            }
            catch (Exception e)
            {
                if (e is TdException etd) OnError?.Invoke(this, etd);
                else OnUnknownError?.Invoke(this, e);
                shouldRetry = tryAgain;
            }
        }
    }
    /// <summary>
    /// Handles the state where TDLib parameters need to be set during the authorization process.
    /// </summary>
    private async Task AuthorizationStateWaitTdlibParametersHandler()
    {
        try
        {
            await client.ExecuteAsync(new TdApi.SetTdlibParameters
            {
                ApiId = apiId,
                ApiHash = apiHash,
                DeviceModel = deviceName,
                SystemLanguageCode = languageCode,
                ApplicationVersion = appVersion,
                DatabaseDirectory = dbLocation,
                //DatabaseEncryptionKey = null,
                UseChatInfoDatabase = true,
                UseMessageDatabase = true,
            });
        }
        catch (Exception e)
        {
            if (e is TdException etd) OnError?.Invoke(this, etd);
            else OnUnknownError?.Invoke(this, e);
        }
    }
    /// <summary>
    /// Handles the state where the user needs to provide an authentication code.
    /// If an error occurs, the operation may be retried based on the <see cref="tryAgain"/> flag.
    /// </summary>
    private async Task AuthorizationStateWaitCodeHandler()
    {
        bool shouldRetry = true;
        while (shouldRetry)
        {
            try
            {
                await HandleUserLoginCode(OnNeedCode(this));
            }
            catch (Exception e)
            {
                if (e is TdException etd) OnError?.Invoke(this, etd);
                else OnUnknownError?.Invoke(this, e);
                shouldRetry = tryAgain;
            }
        }
    }
    /// <summary>
    /// Handles the state where the user needs to provide a password for authentication.
    /// If an error occurs, the operation may be retried based on the <see cref="tryAgain"/> flag.
    /// </summary>
    private async Task AuthorizationStateWaitPasswordHandler()
    {
        bool shouldRetry = true;
        while (shouldRetry)
        {
            try
            {
                await HandleUserLoginPassword(OnNeedPassword(this));
                break;
            }
            catch (Exception e)
            {
                if (e is TdException etd) OnError?.Invoke(this, etd);
                else OnUnknownError?.Invoke(this, e);
                shouldRetry = tryAgain;
            }
        }
    }
    /// <summary>
    /// Event handler for updates received from the TDLib client.
    /// Specifically handles updates related to changes in the authorization state.
    /// </summary>
    /// <param name="sender">The sender of the update event.</param>
    /// <param name="e">The update received from TDLib.</param>
    private async void Client_UpdateReceived(object? sender, TdApi.Update e)//event handler
    {
        switch (e)
        {
            case TdApi.Update.UpdateAuthorizationState:
                {
                    TdApi.AuthorizationState authorizationState = ((TdApi.Update.UpdateAuthorizationState)e).AuthorizationState;
                    await HandleUpdateAuthorizationState(authorizationState);
                    break;
                }
        }
    }
    #endregion
    #region PUBLIC
    /// <summary>
    /// Blocks the caller thread until the authorization process is completed and the client is ready to use.
    /// </summary>
    public void Wait()
    {
        Ready.Wait();
    }
    /// <summary>
    /// Forcefully negates the effect of the <see cref="Wait"/> function.
    /// </summary>
    public void Relase()
    {
        Ready.Set();
    }
    /// <summary>
    /// Remove update event handler
    /// </summary>
    public void Dispose()
    {
        client.UpdateReceived -= Client_UpdateReceived;
    }
    #endregion
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationHandler"/> class to handle the authorization process in Telegram.
    /// </summary>
    /// <param name="client">The <see cref="TdClient"/> instance used for communication with the Telegram API. The TdClient instance created must be entirely new and should not have been authenticated previously.</param>
    /// <param name="dbLocation">The file path where the database should be stored.</param>
    /// <param name="appVersion">The version of the application.</param>
    /// <param name="data">The phone number or email address used for authentication, depending on the login method.</param>
    /// <param name="apiHash">The API hash provided by Telegram for the application.</param>
    /// <param name="apiId">The API ID provided by Telegram for the application.</param>
    /// <param name="deviceName">The name of the device where the application is running.</param>
    /// <param name="languageCode">The code of the language used for the application (e.g., "en" for English).</param>
    /// <param name="OnNeedFirstName">A callback function that returns the first name when user registration is required.</param>
    /// <param name="OnNeedLastName">A callback function that returns the last name when user registration is required.</param>
    /// <param name="OnNeedCode">A callback function that returns the authentication code sent via SMS or email.</param>
    /// <param name="OnNeedPassword">A callback function that returns the password when 2FA (Two-Factor Authentication) is enabled.</param>
    /// <param name="OnUnknownError">An optional callback function that is invoked when an unknown error occurs during the authorization process.</param>
    /// <param name="OnError">An optional callback function that is invoked when a specific <see cref="TdException"/> error occurs during the authorization process.</param>
    /// <param name="OnReadyToUse">An optional callback function that is invoked when the authorization process is successfully completed and the client is ready to use.</param>
    /// <param name="loginMethod">Specifies the login method (<see cref="LoginMethod.PhoneNumber"/> or <see cref="LoginMethod.Email"/>) for authentication. Defaults to <see cref="LoginMethod.PhoneNumber"/>.</param>
    /// <param name="tryAgain">Indicates whether to retry the authorization process in case of failure. Defaults to true.</param>
    public AuthorizationHandler(TdClient client, string dbLocation, string appVersion, string data,
                                string apiHash, int apiId, string deviceName, string languageCode,
                                Func<AuthorizationHandler, string> OnNeedCode, Func<AuthorizationHandler, string> OnNeedPassword,
                                Action<AuthorizationHandler, Exception>? OnUnknownError = null, Action<AuthorizationHandler, TdException>? OnError = null,
                                Action<AuthorizationHandler>? OnReadyToUse = null,
                                LoginMethod loginMethod = LoginMethod.PhoneNumber, bool tryAgain = true)
    {
        this.client = client;
        this.dbLocation = dbLocation;
        this.appVersion = appVersion;
        this.data = data;
        this.apiHash = apiHash;
        this.apiId = apiId;
        this.deviceName = deviceName;
        this.languageCode = languageCode;
        this.loginMethod = loginMethod;
        this.tryAgain = tryAgain;
        this.OnError = OnError;
        this.OnUnknownError = OnUnknownError;
        this.OnReadyToUse = OnReadyToUse;
        this.OnNeedPassword = OnNeedPassword;
        this.OnNeedCode = OnNeedCode;
        client.UpdateReceived += Client_UpdateReceived;
    }
}