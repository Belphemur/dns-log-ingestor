using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace VictoriaMetrics.VictoriaMetrics.Client.Content
{
    /// <summary>
    /// https://github.com/WebApiContrib/WebAPIContrib/blob/master/src/WebApiContrib/Content/CompressedContent.cs
    /// </summary>
    internal class CompressedContent : HttpContent
    {
        private readonly HttpContent _originalContent;
        private readonly Compression _encodingType;

        public enum Compression
        {
            gzip,
            deflate
        }

        public CompressedContent(HttpContent content, Compression encodingType)
        {
            _originalContent = content ?? throw new ArgumentNullException(nameof(content));

            if (this._encodingType != Compression.gzip && this._encodingType != Compression.deflate)
            {
                throw new InvalidOperationException($"Encoding '{this._encodingType}' is not supported. Only supports {nameof(Compression.gzip)} or {nameof(Compression.deflate)} encoding.");
            }

            foreach (var (key, value) in _originalContent.Headers)
            {
                Headers.TryAddWithoutValidation(key, value);
            }

            Headers.ContentEncoding.Add(encodingType.ToString());
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = null;

            switch (_encodingType)
            {
                case Compression.gzip:
                    compressedStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
                    break;
                case Compression.deflate:
                    compressedStream = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
                    break;
            }

            return _originalContent.CopyToAsync(compressedStream).ContinueWith(tsk => { compressedStream?.Dispose(); });
        }
    }
}