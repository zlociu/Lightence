using System;
using System.Collections.Generic;
using System.Text;
using MaterialDesignThemes.Wpf;

namespace LightenceClient
{
    public static class ExtensionIconCheck
    {
        public static PackIconKind GetIcon(string fileExtension)
        {
            PackIconKind iconKind;
            switch(fileExtension)
            {
                case ".docx":
                case ".xlsx":
                case ".pptx":
                case ".txt": iconKind = PackIconKind.FileDocumentOutline; break;
                case ".pdf": iconKind = PackIconKind.FileAcrobatOutline; break;
                case ".c" :
                case ".cpp":
                case ".cs":
                case ".fsx":
                case ".java":
                case ".py":
                case ".html":
                case ".htm":
                case ".css":
                case ".xaml": iconKind = PackIconKind.FileCodeOutline; break;
                case ".png":
                case ".jpg":
                case ".bmp":
                case ".svg": iconKind = PackIconKind.FileImageOutline; break;
                case ".cj": iconKind = PackIconKind.FileRemoveOutline; break;
                default: iconKind = PackIconKind.FileDocumentOutline; break;
            }
            return iconKind;
        }
    }
}
