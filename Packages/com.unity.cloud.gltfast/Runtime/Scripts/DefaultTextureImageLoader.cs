// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Logging;
using GLTFast.Schema;
using Unity.Collections;

namespace GLTFast
{
    /// <summary>
    /// Public access to glTFast's built-in image loading (PNG/JPEG via the ImageConversion module and, when the
    /// KTX for Unity package is installed, KTX).
    /// Intended to be wrapped by custom <see cref="ITextureImageLoader"/> add-ons that want to adjust the
    /// default behavior (e.g. force readable textures) without re-implementing image decoding.
    /// </summary>
    /// <remarks>
    /// On its own, this loader does not claim any glTF texture
    /// (<see cref="IsAbleToLoad(TextureBase,out int)"/> returns false); wrappers decide which textures to handle.
    /// </remarks>
    public class DefaultTextureImageLoader : ITextureImageLoader
    {
        readonly ImportContext m_Context;
        readonly int m_AnisotropicFilterLevel;

        /// <summary>
        /// Creates a loader that uses glTFast's built-in image decoding.
        /// </summary>
        /// <param name="settings">Import settings. Only <see cref="ImportSettings.AnisotropicFilterLevel"/> is used;
        /// defaults are applied if null.</param>
        /// <param name="deferAgent">Defer agent used to spread work across frames. If null, work is not deferred.</param>
        /// <param name="logger">Logger for load errors. Optional.</param>
        public DefaultTextureImageLoader(
            ImportSettings settings = null,
            IDeferAgent deferAgent = null,
            ICodeLogger logger = null
            )
        {
            m_AnisotropicFilterLevel = (settings ?? new ImportSettings()).AnisotropicFilterLevel;
            m_Context = new ImportContext(null, logger, deferAgent ?? new UninterruptedDeferAgent());
        }

        /// <inheritdoc />
        public Task<ImageResult> LoadImage(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
            )
        {
            var span = data.AsReadOnlySpan();

            if (ImageFormatDetection.IsPngOrJpeg(span))
            {
#if UNITY_IMAGECONVERSION
                return ImageConversionImageLoader.LoadAsync(
                    m_Context, data, linear, readable, generateMipMaps, m_AnisotropicFilterLevel, cancellationToken);
#else
                m_Context.Logger?.Error(LogCode.ImageConversionNotEnabled);
                return Task.FromResult(ImageResult.Null);
#endif
            }

            if (ImageFormatDetection.IsKtx(span))
            {
#if KTX_IS_RECENT
                return KtxImageLoader.LoadAsync(
                    m_Context,
                    new ImportSettings { GenerateMipMaps = generateMipMaps },
                    data,
                    linear,
                    readable,
                    cancellationToken);
#else
                m_Context.Logger?.Error(
                    LogCode.PackageMissing, "KTX for Unity", ExtensionName.TextureBasisUniversal);
                return Task.FromResult(ImageResult.Null);
#endif
            }

            m_Context.Logger?.Error(LogCode.ImageFormatUnknown, "?");
            return Task.FromResult(ImageResult.Null);
        }

        /// <summary>
        /// Does not claim any glTF texture. Wrapping loaders decide which textures they handle.
        /// </summary>
        /// <param name="texture">glTF texture.</param>
        /// <param name="imageIndex">Always -1.</param>
        /// <returns>Always false.</returns>
        public bool IsAbleToLoad(TextureBase texture, out int imageIndex)
        {
            imageIndex = -1;
            return false;
        }

        /// <inheritdoc />
        public bool IsAbleToLoad(ReadOnlySpan<byte> data)
        {
            return ImageFormatDetection.IsPngOrJpeg(data)
#if KTX_IS_RECENT
                || ImageFormatDetection.IsKtx(data)
#endif
                ;
        }
    }
}
