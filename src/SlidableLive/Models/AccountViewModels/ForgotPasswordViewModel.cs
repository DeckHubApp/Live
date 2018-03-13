﻿using System.ComponentModel.DataAnnotations;

namespace SlidableLive.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
