﻿using SmartStore.Models;
using SmartStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SmartStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var user = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Adress = registerDto.Address,
                CreatedAt = DateTime.Now,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                // successful registration 
                await _userManager.AddToRoleAsync(user, "client");
                // sign in the new user
                await _signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            // registration failed => erros
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDto);
        }

        public async Task<IActionResult> Logout()
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password,
                loginDto.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login attempt.";
            }

            return View(loginDto);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var profileDto = new ProfileDto()
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? "",
                PhoneNumber = appUser.PhoneNumber,
                Address = appUser.Adress,
            };

            return View(profileDto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto profileDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill all the required fields with valid values";
                return View(profileDto);
            }

            var appUser = await _userManager.GetUserAsync(User);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            appUser.FirstName = profileDto.FirstName;
            appUser.LastName = profileDto.LastName;
            appUser.UserName = profileDto.Email;
            appUser.Email = profileDto.Email;
            appUser.PhoneNumber = profileDto.PhoneNumber;
            appUser.Adress = profileDto.Address;

            var result = await _userManager.UpdateAsync(appUser);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Profile updated successfuly";
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to update the profile:" + result.Errors.First().Description;
            }

            return View(profileDto);
        }

        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Password(PasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ChangePasswordAsync(appUser,
                passwordDto.CurrentPassword, passwordDto.NewPassword);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Profile updated successfuly";
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to update the profile:" + result.Errors.First().Description;
            }

            return View();
        }

        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([Required, EmailAddress] string email)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = email;

            if (!ModelState.IsValid)
            {
                ViewBag.EmailError = ModelState["email"]?.Errors.First().ErrorMessage ?? "Invalid Email Address";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string resetUrl = Url.ActionLink("ResetPassword", "Account", new { token }) ?? "URL Error";

                // send url by email
                string senderName = _configuration["BrevoSettings:SenderName"] ?? "";
                string senderEmail = _configuration["BrevoSettings:SenderEmail"] ?? "";
                string username = user.FirstName + " " + user.LastName;
                string subject = "Password Reset";
                string message = "Dear " + username + ",\n\n" +
                                 "You can reset your password using the following link:\n\n" +
                                 resetUrl + "\n\n" +
                                 "Best Regards";

                EmailSender.SendEmail(senderName, senderEmail, username, email, subject, message);
            }

            ViewBag.SuccessMessage = "Please check your Email account and click on the Password Reset link!";

            return View();
        }

        public IActionResult ResetPassword(string? token)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(string? token, PasswordResetDto model)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Token not valid!";
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Password reset successfully!";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
