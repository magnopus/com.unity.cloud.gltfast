// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast
{
    /// <summary>
    /// Result returned by <see cref="ITextureCreator.CreateTextureAsync"/>.
    /// Carries the created texture together with image metadata that callers may need.
    /// Additional metadata fields can be added here in the future without changing the method signature.
    /// </summary>
    public readonly struct TextureCreationResult
    {
        /// <summary>The created texture, or null if creation failed.</summary>
        public readonly Texture2D Texture;

        /// <summary>
        /// When true, the image data is Y-flipped relative to Unity's convention
        /// (i.e. stored top-to-bottom) and the texture may need to be flipped on use.
        /// Always false for JPEG/PNG; derived from <c>KtxOrientation.IsYFlipped()</c> for KTX2.
        /// </summary>
        public readonly bool FlipY;

        /// <param name="texture">The created texture.</param>
        /// <param name="flipY">True if the texture is Y-flipped relative to Unity's convention.</param>
        public TextureCreationResult(Texture2D texture, bool flipY = false)
        {
            Texture = texture;
            FlipY = flipY;
        }
    }
}
