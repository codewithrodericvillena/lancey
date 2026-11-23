using Avalonia.Media.Imaging;
using Lancy.Avalonia.ViewModels.Shared;
using Lancy.Core.Entities;

namespace Lancy.Avalonia.ViewModels.Clients;

/// <summary>Pairs a ClientAttachment (e.g. a signed contract) with its loaded preview. See AttachmentPreviewLoader for the image-vs-document distinction.</summary>
public class ClientAttachmentItemViewModel
{
    public ClientAttachment Attachment { get; }
    public Bitmap? Image { get; }
    public bool IsImage { get; }
    public bool LoadFailed => IsImage && Image is null;

    public ClientAttachmentItemViewModel(ClientAttachment attachment, string filePath)
    {
        Attachment = attachment;
        (IsImage, Image) = AttachmentPreviewLoader.Load(attachment.ContentType, filePath);
    }
}
