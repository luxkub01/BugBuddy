// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BugBuddy.Data;
using BugBuddy.Helpers;
using BugBuddy.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BugBuddy.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _sender;
        private readonly SmtpSettings _smtpSettings;
        public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender, SmtpSettings smtpSettings)
        {
            _userManager = userManager;
            _sender = sender;
            _smtpSettings = smtpSettings;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                // Prepare the user confirmation email
                var confirmationEmail = new SMTPEmailObject
                {
                    Port = _smtpSettings.Port > 0 ? _smtpSettings.Port : 587,
                    Server = _smtpSettings.Host,
                    From = _smtpSettings.UserName,
                    Username = _smtpSettings.UserName,
                    Password = _smtpSettings.Password,

                    To = user.Email,
                    Subject = "Confirm your email - VetHunt",
                    Body = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>
                                <h2 style='color: #333;'>Welcome to VetHunt!</h2>
                                <p>Hi {user.FirstName},</p>
                                <p>Thank you for signing up. Please confirm your email address by clicking the button below:</p>

                                <p style='margin: 20px 0;'>
                                    <a href='{HtmlEncoder.Default.Encode(EmailConfirmationUrl)}'
                                       style='background-color:#28a745;color:#fff;text-decoration:none;padding:10px 20px;border-radius:5px;'>
                                       Confirm Email
                                    </a>
                                </p>

                                <p>If the button above doesn't work, copy and paste this link into your browser:</p>
                                <p style='color:#555;'>{HtmlEncoder.Default.Encode(EmailConfirmationUrl)}</p>

                                <p style='margin-top:30px;'>If you didn’t request this, just ignore this email.</p>
                                <p>Regards,<br>VetHunt Team</p>
                            </div>"
                };


                var response = await SMTP.SendEmailAsync(confirmationEmail);
                if (response)
                    DisplayConfirmAccountLink = false;

            }

            return Page();
        }
    }
}
