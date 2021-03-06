﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Validation;
using System.Web.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi.Filters;
using IModelBinder = System.Web.Http.ModelBinding.IModelBinder;
using ModelBindingContext = System.Web.Http.ModelBinding.ModelBindingContext;
using ModelMetadata = System.Web.Http.Metadata.ModelMetadata;
using ModelMetadataProvider = System.Web.Http.Metadata.ModelMetadataProvider;
using MutableObjectModelBinder = System.Web.Http.ModelBinding.Binders.MutableObjectModelBinder;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Web.WebApi.Binders
{
    /// <summary>
    /// Binds the content model to the controller action for the posted multi-part Post
    /// </summary>
    internal abstract class ContentItemBaseBinder<TPersisted> : IModelBinder
        where TPersisted : IContentBase
    {
        protected ApplicationContext ApplicationContext { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="applicationContext"></param>
        internal ContentItemBaseBinder(ApplicationContext applicationContext)
        {
            ApplicationContext = applicationContext;
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            //NOTE: Validation is done in the filter
            if (actionContext.Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = HttpContext.Current.Server.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var task = Task.Run(() => GetModel(actionContext, bindingContext, provider))
                           .ContinueWith(x =>
                               {
                                   if (x.IsFaulted && x.Exception != null)
                                   {
                                       throw x.Exception;
                                   }

                                   //now that everything is binded, validate the properties
                                   var contentItemValidator = new ContentItemValidationHelper<TPersisted>(ApplicationContext);
                                   contentItemValidator.ValidateItem(actionContext, x.Result);

                                   bindingContext.Model = x.Result;
                               });

            task.Wait();

            return bindingContext.Model != null;
        }

        /// <summary>
        /// Builds the model from the request contents
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private async Task<ContentItemSave<TPersisted>> GetModel(HttpActionContext actionContext, ModelBindingContext bindingContext, MultipartFormDataStreamProvider provider)
        {
            var request = actionContext.Request;

            //IMPORTANT!!! We need to ensure the umbraco context here because this is running in an async thread
            UmbracoContext.EnsureContext(request.Properties["MS_HttpContext"] as HttpContextBase, ApplicationContext.Current);

            var content = request.Content;

            var result = await content.ReadAsMultipartAsync(provider);

            if (result.FormData["contentItem"] == null)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            ReasonPhrase = "The request was not formatted correctly and is missing the 'contentItem' parameter"
                        });
            }

            //get the string json from the request
            var contentItem = result.FormData["contentItem"];

            //deserialize into our model
            var model = JsonConvert.DeserializeObject<ContentItemSave<TPersisted>>(contentItem);
            
            //get the default body validator and validate the object
            var bodyValidator = actionContext.ControllerContext.Configuration.Services.GetBodyModelValidator();
            var metadataProvider = actionContext.ControllerContext.Configuration.Services.GetModelMetadataProvider();
            //all validation errors will not contain a prefix
            bodyValidator.Validate(model, typeof (ContentItemSave<TPersisted>), metadataProvider, actionContext, "");

            //get the files
            foreach (var file in result.FileData)
            {
                //The name that has been assigned in JS has 2 parts and the second part indicates the property id 
                // for which the file belongs.
                var parts = file.Headers.ContentDisposition.Name.Trim(new char[] { '\"' }).Split('_');
                if (parts.Length != 2)
                {
                    throw new HttpResponseException(
                        new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                ReasonPhrase = "The request was not formatted correctly the file name's must be underscore delimited"
                            });
                }
                int propertyId;
                if (int.TryParse(parts[1], out propertyId) == false)
                {
                    throw new HttpResponseException(
                        new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                ReasonPhrase = "The request was not formatted correctly the file name's 2nd part must be an integer"
                            });
                }

                var fileName = file.Headers.ContentDisposition.FileName.Trim(new char[] {'\"'});

                model.UploadedFiles.Add(new ContentItemFile
                    {
                        TempFilePath = file.LocalFileName,
                        PropertyId = propertyId,
                        FileName = fileName
                    });
            }

            if (model.Action == ContentSaveAction.Publish || model.Action == ContentSaveAction.Save)
            {
                //finally, let's lookup the real content item and create the DTO item
                model.PersistedContent = GetExisting(model);
            }
            else
            {
                //we are creating new content                          
                model.PersistedContent = CreateNew(model);
            }

            //create the dto from the persisted model
            model.ContentDto = MapFromPersisted(model);
            
            //now map all of the saved values to the dto
            MapPropertyValuesFromSaved(model, model.ContentDto);
            
            return model;
        }

        /// <summary>
        /// we will now assign all of the values in the 'save' model to the DTO object
        /// </summary>
        /// <param name="saveModel"></param>
        /// <param name="dto"></param>
        private static void MapPropertyValuesFromSaved(ContentItemSave<TPersisted> saveModel, ContentItemDto<TPersisted> dto)
        {
            foreach (var p in saveModel.Properties)
            {
                dto.Properties.Single(x => x.Alias == p.Alias).Value = p.Value;
            }
        }

        protected abstract TPersisted GetExisting(ContentItemSave<TPersisted> model);
        protected abstract TPersisted CreateNew(ContentItemSave<TPersisted> model);
        protected abstract ContentItemDto<TPersisted> MapFromPersisted(ContentItemSave<TPersisted> model);
    }
}