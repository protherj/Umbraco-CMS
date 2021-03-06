﻿using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.WebApi.Binders
{
    internal class MediaItemBinder : ContentItemBaseBinder<IMedia>
    {
        private readonly MediaModelMapper _mediaModelMapper;

        public MediaItemBinder(ApplicationContext applicationContext, MediaModelMapper mediaModelMapper)
            : base(applicationContext)
        {
            _mediaModelMapper = mediaModelMapper;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaItemBinder()
            : this(ApplicationContext.Current, new MediaModelMapper(ApplicationContext.Current, new UserModelMapper()))
        {
        }

        protected override IMedia GetExisting(ContentItemSave<IMedia> model)
        {
            return ApplicationContext.Services.MediaService.GetById(model.Id);
        }

        protected override IMedia CreateNew(ContentItemSave<IMedia> model)
        {
            var contentType = ApplicationContext.Services.ContentTypeService.GetMediaType(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No content type found wth alias " + model.ContentTypeAlias);
            }
            return new Core.Models.Media(model.Name, model.ParentId, contentType);
        }

        protected override ContentItemDto<IMedia> MapFromPersisted(ContentItemSave<IMedia> model)
        {
            return _mediaModelMapper.ToMediaItemDto(model.PersistedContent);
        }
    }
}