using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class BaseValidationViewModel
    {
        public IEnumerable<string> Errors { get; set; }

        public bool Success
        {
            get { return Errors == null || !Errors.Any(); }
        }
    }

    public static class BaseValidationViewModelExtensions
    {
        public static JsonResult GetJsonResultWithValidationErrors(this ModelStateDictionary modelState)
        {
            return GetJsonResultWithValidationErrors(modelState, new BaseValidationViewModel());
        }

        public static JsonResult GetJsonResultWithValidationErrors(this ModelStateDictionary modelState, BaseValidationViewModel response)
        {
            response = response ?? new BaseValidationViewModel();

            if (!modelState.IsValid)
                response.Errors = modelState.Values.SelectMany(ms => ms.Errors).Select(me => me.Exception?.Message ?? me.ErrorMessage);

            var result = new JsonResult(response, new JsonSerializerSettings { Formatting = Formatting.Indented });

            result.StatusCode = (int)(response.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);

            return result;
        }

    }
}
