using Aloha.ServiceDefaults.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Aloha.ServiceDefaults.Extensions
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        // This class is used to validate the model attributes
        // If the model is not valid, it will return a bad request response
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var errors = new Dictionary<string, string[]>();

            // Always run this validation, even if the model state is invalid
            if (!context.ModelState.IsValid)
            {
                foreach (var modelStateEntry in context.ModelState)
                {
                    var key = modelStateEntry.Key;
                    var errorMessages = new List<string>();

                    foreach (var error in modelStateEntry.Value.Errors)
                    {
                        if (!string.IsNullOrEmpty(error.ErrorMessage))
                        {
                            errorMessages.Add(error.ErrorMessage);
                        }
                        else if (error.Exception != null)
                        {
                            // Check for specific exceptions related to invalid data types
                            if (error.Exception is FormatException || error.Exception is InvalidCastException)
                            {
                                string propertyName = key.Split('.').Last();
                                string customMessage = GetCustomTypeErrorMessage(propertyName, context.ActionArguments);
                                errorMessages.Add(customMessage);
                            }
                            else
                            {
                                errorMessages.Add(error.Exception.Message);
                            }
                        }
                    }

                    // Add default required field message if no errors were added but the field is invalid
                    if (!errorMessages.Any() && modelStateEntry.Value.ValidationState == ModelValidationState.Invalid)
                    {
                        string propertyName = key.Split('.').Last();
                        errorMessages.Add($"The {propertyName} field is required.");
                    }

                    if (errorMessages.Any())
                    {
                        errors[key] = errorMessages.ToArray();
                    }
                }

                var response = ApiResponseBuilder.BuildResponse(
                    data: errors,
                    message: "Validation failed"
                );

                context.Result = new BadRequestObjectResult(response);
            }
        }

        private string GetCustomTypeErrorMessage(string propertyName, IDictionary<string, object> actionArguments)
        {
            // Try to get the expected type from the action parameters
            var parameter = actionArguments.FirstOrDefault(p => p.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (parameter.Value != null)
            {
                Type expectedType = parameter.Value.GetType();
                return expectedType.Name switch
                {
                    nameof(Int32) or nameof(Int64) or nameof(Double) or nameof(Decimal)
                        => $"The field {propertyName} must be a valid number.",
                    nameof(DateTime) => $"The field {propertyName} must be a valid date and time.",
                    nameof(Boolean) => $"The field {propertyName} must be true or false.",
                    nameof(Guid) => $"The field {propertyName} must be a valid GUID.",
                    _ when expectedType.IsEnum => $"The field {propertyName} must be one of the allowed values.",
                    _ => $"The field {propertyName} has an invalid value."
                };
            }

            return $"The field {propertyName} has an invalid value.";
        }
    }
}
