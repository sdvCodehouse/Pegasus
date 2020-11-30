﻿using System.Threading.Tasks;
using Pegasus.Library.Models.Manage;

namespace Pegasus.Library.Api
{
    public class ManageEndpoint : IManageEndpoint
    {
        private readonly IApiHelper _apiHelper;

        public ManageEndpoint(IApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<TwoFactorAuthenticationModel> TwoFactorAuthentication(string email)
        {
            return await _apiHelper.GetFromUri<TwoFactorAuthenticationModel>($"api/Account/Manage/TwoFactorAuthentication/{email}");
        }

        //TODO ForgetBrowser is not going to work across api
        public async Task ForgetTwoFactorClientAsync(string email)
        {
            await _apiHelper.PostAsync(email,$"api/Account/Manage/ForgetTwoFactorClient");
        }

        public async Task<AuthenticatorKeyModel> LoadSharedKeyAndQrCodeUriAsync(string email)
        {
            return await _apiHelper.GetFromUri<AuthenticatorKeyModel>($"api/Account/Manage/LoadSharedKeyAndQrCodeUri/{email}");
        }

        public async Task<VerifyTwoFactorTokenModel> VerifyTwoFactorTokenAsync(VerifyTwoFactorTokenModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/VerifyTwoFactorToken");
        }

        public async Task<SetTwoFactorEnabledModel> SetTwoFactorEnabledAsync(SetTwoFactorEnabledModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/SetTwoFactorEnabled");
        }

        public async Task<GetTwoFactorEnabledModel> GetTwoFactorEnabledAsync(string email)
        {
            return await _apiHelper.GetFromUri<GetTwoFactorEnabledModel>($"api/Account/Manage/GetTwoFactorEnabled/{email}");
        }

        public async Task<GenerateRecoveryCodesModel> GenerateNewRecoveryCodesAsync(string email)
        {
            return await _apiHelper.GetFromUri<GenerateRecoveryCodesModel>($"api/Account/Manage/GenerateNewRecoveryCodes/{email}");
        }

        public async Task<RecoveryCodeStatusModel> CheckRecoveryCodesStatus(RecoveryCodeStatusModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/CheckRecoveryCodesStatus");
        }

        public async Task<UserDetailsModel> GetUserDetails(string userId)
        {
            return await _apiHelper.GetFromUri<UserDetailsModel>($"api/Account/Manage/GetUserDetails/{userId}");
        }

        public async Task<UserDetailsModel> SetUserDetails(UserDetailsModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/SetUserDetails");
        }

        public async Task<HasPasswordModel> HasPasswordAsync(HasPasswordModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/HasPassword");
        }

        public async Task<ChangePasswordModel> ChangePasswordAsync(ChangePasswordModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/ChangePassword");
        }

        public async Task<SetPasswordModel> AddPasswordAsync(SetPasswordModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/AddPassword");
        }

        public async Task<ResetAuthenticatorModel> ResetAuthenticatorAsync(ResetAuthenticatorModel model)
        {
            return await _apiHelper.PostAsync(model,$"api/Account/Manage/ResetAuthenticator");
        }
    }
}
