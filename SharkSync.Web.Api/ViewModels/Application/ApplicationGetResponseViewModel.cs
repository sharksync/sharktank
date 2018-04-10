using Newtonsoft.Json;
using SharkTank.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class ApplicationGetResponseViewModel : BaseValidationViewModel
    {
        public ApplicationViewModel Application { get; set; }
    }

    public class ApplicationViewModel
    {
        public Guid Id { get; set; }
        public Guid AccessKey { get; set; }
        public string Name { get; set; }

        public ApplicationViewModel(IApplication app)
        {
            Id = app.Id;
            AccessKey = app.AccessKey;
            Name = app.Name;
        }
    }
}
