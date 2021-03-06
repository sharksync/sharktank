﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class AuthDetailsViewModel : BaseValidationViewModel
    {
        public UserDetailsViewModel LoggedInUser { get; set; }
    }

    public class UserDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string AccountType { get; set; }
        public bool HasAvatarUrl { get; set; }
        public string XSRFToken { get; set; }
    }
}
