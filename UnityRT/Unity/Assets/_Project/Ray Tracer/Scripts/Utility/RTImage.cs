using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Utility
{
    /// <summary>
    /// Stores a small output image from the ray tracer as well as auxiliary information. Used to display a preview 
    /// image of the ray tracer result in the UI or on the virtual screen in the scene. Should not be used for storing
    /// full resolution renders.
    /// </summary>
    public class RTImage
    {
        public delegate void ImageChanged();
        /// <summary>
        /// An event invoked whenever a property of this image is changed.
        /// </summary>
        public event ImageChanged OnImageChanged;

        private int width;
        /// <summary>
        /// The width of the stored image in pixels.
        /// </summary>
        public int Width
        {
            get { return width; }
            private set { width = value > 0 ? value : 0; }
        }

        private int height;
        /// <summary>
        /// The height of the stored image in pixels.
        /// </summary>
        public int Height
        {
            get { return height; }
            private set { height = value > 0 ? value : 0; }
        }

        private Color[] pixels;
        /// <summary>
        /// The raw image data in the form of a flattened array of pixel colors. The pixels are laid out left to right,
        /// bottom to top (i.e. row after row). Setting the array to one of a larger size will result in only the first
        /// <c><see cref="Width"/> * <see cref="Height"/></c> colors being copied over. Setting the array to one of smaller
        /// size will log a warning and do nothing. Changing the dimensions of the image can be done by using 
        /// <see cref="Reset(int, int)"/> or <see cref="Reset(int, int, Color[])"/>.
        /// </summary>
        public Color[] Pixels
        {
            get { return pixels; }
            set 
            { 
                // We should be provided enough pixels.
                if (value.Length < Width * Height)
                {
                    Debug.LogWarning("Cannot assign " + value.Length + " pixels to an image with " + Width * Height + 
                                     " pixels. Ignoring assignment.");
                    return;
                }

                // Make sure the array of pixels has the right size.
                if (pixels.Length != Width * Height)
                    pixels = new Color[Width * Height];
            
                // Set the array of pixels to the first part of the provided pixel array.
                for (int index = 0; index < Width * Height; ++index)
                    pixels[index] = value[index];
            }
        }

        private Texture2D texture;

        /// <summary>
        /// Create a new black image.
        /// </summary>
        /// <param name="width"> The width of the new image in pixels. </param>
        /// <param name="height"> The height of the new image in pixels. </param>
        public RTImage(int width, int height)
        {
            Width = width;
            Height = height;

            pixels = new Color[Width * Height];
            SetUniformColor(Color.black);
            UpdateTexture();
        }

        /// <summary>
        /// Create a new image. If the length of <paramref name="pixels"/> is less than 
        /// <c><paramref name="width"/> * <paramref name="height"/></c> the image will be black.
        /// </summary>
        /// <param name="width"> The width of the new image in pixels. </param>
        /// <param name="height"> The height of the new image in pixels. </param>
        /// <param name="pixels"> The raw image data in the form of a flattened array of pixel colors. </param>
        public RTImage(int width, int height, Color[] pixels)
        {
            Width = width;
            Height = height;

            this.pixels = new Color[Width * Height];
            SetUniformColor(Color.black); // Default to black in case assignment fails.
            Pixels = pixels;
            UpdateTexture();
        }

        /// <summary>
        /// Reset this image to have new dimensions and make all pixels black.
        /// </summary>
        /// <param name="width"> The new width of the image in pixels. </param>
        /// <param name="height"> The new height of the image in pixels. </param>
        public void Reset(int width, int height)
        {
            Width = width;
            Height = height;

            // Only remake the array if it is not the right size already.
            if (pixels.Length != Width * Height)
                pixels = new Color[Width * Height];
            SetUniformColor(Color.black);

            UpdateTexture();
            OnImageChanged?.Invoke();
        }

        /// <summary>
        /// Reset this image to have new dimensions and pixel colors. If the length of <paramref name="pixels"/> is less 
        /// than <c><paramref name="width"/> * <paramref name="height"/></c> the image will be black.
        /// </summary>
        /// <param name="width"> The new width of the image in pixels. </param>
        /// <param name="height"> The new height of the image in pixels. </param>
        /// <param name="pixels"> The new raw image data in the form of a flattened array of pixel colors. </param>
        public void Reset(int width, int height, Color[] pixels)
        {
            Width = width;
            Height = height;

            // Only remake the array if it is not the right size already.
            if (this.pixels.Length != Width * Height)
                this.pixels = new Color[Width * Height];
            SetUniformColor(Color.black); // Default to black in case assignment fails.
            Pixels = pixels;

            UpdateTexture();
            OnImageChanged?.Invoke();
        }

        /// <summary>
        /// Reset this image to have new dimensions and pixel colors. The colors of the pixels are determined from the
        /// <paramref name="bytes"/> byte array. Each byte represents one color channel. Three color channels make up an
        /// RGB color and four make an RGBA color. If the length of <paramref name="bytes"/> is not
        /// <c><paramref name="width"/> * <paramref name="height"/> * 3</c> for RGB colors or
        /// <c><paramref name="width"/> * <paramref name="height"/> * 4</c> for RGBA colors the image will be black.
        /// </summary>
        /// <param name="width"> The new width of the image in pixels. </param>
        /// <param name="height"> The new height of the image in pixels. </param>
        /// <param name="bytes"> The byte array storing a flattened array of pixel colors. </param>
        /// <param name="alpha"> 
        /// Whether the specified colors are RGBA (with alpha channel) or RGB (without alpha channel).
        /// </param>
        public void Reset(int width, int height, byte[] bytes, bool alpha = false)
        {
            Width = width;
            Height = height;

            // Make sure we got the right number of bytes, otherwise we set the image to black.
            int size = alpha ? width * height * 4 : width * height * 3; // Either four or three 8 bit channels.
            if (bytes.Length != size)
            {
                Debug.LogWarning("Cannot construct an image from " + bytes.Length + " bytes when " + size + 
                                 "were expected.");
                SetUniformColor(Color.black);
                UpdateTexture();
                OnImageChanged?.Invoke();
                return;
            }

            // Only remake the array if it is not the right size already.
            if (pixels.Length != Width * Height)
                pixels = new Color[Width * Height];
            SetUniformColor(Color.black); // Default to black in case assignment fails.

            if (alpha)
            {
                for (int i = 0; i < width * height; ++i)
                {
                    float r = bytes[4 * i + 0] / 255.0f;
                    float g = bytes[4 * i + 1] / 255.0f;
                    float b = bytes[4 * i + 2] / 255.0f;
                    float a = bytes[4 * i + 3] / 255.0f;
                    pixels[i] = new Color(r, g, b, a);
                }
            }
            else
            {
                for (int i = 0; i < width * height; ++i)
                {
                    float r = bytes[3 * i + 0] / 255.0f;
                    float g = bytes[3 * i + 1] / 255.0f;
                    float b = bytes[3 * i + 2] / 255.0f;
                    pixels[i] = new Color(r, g, b, 1.0f);
                }
            }

            UpdateTexture();
            OnImageChanged?.Invoke();
        }

        /// <summary>
        /// Set all pixel in this image to <paramref name="color"/>. Useful for debugging.
        /// </summary>
        /// <param name="color"> The new color for all pixels in the image. </param>
        public void SetUniformColor(Color color)
        {
            for (int index = 0; index < Pixels.Length; index++)
                Pixels[index] = color;

            UpdateTexture();
            OnImageChanged?.Invoke();
        }

        /// <summary>
        /// Set all pixels in this image to random color. Useful for debugging.
        /// </summary>
        public void SetRandomColors()
        {
            for (int index = 0; index < Pixels.Length; ++index)
                Pixels[index] = Random.ColorHSV();

            UpdateTexture();
            OnImageChanged?.Invoke();
        }

        /// <summary>
        /// Get a newly created sprite from this image. The sprite's anchor will be in the center. The texture used by the
        /// returned sprite will get destroyed whenever this image is updated, so be sure to get a new sprite whenever
        /// <see cref="OnImageChanged"/> is invoked. When doing this you must call <see cref="Object.Destroy"/> on the old
        /// sprite to prevent a memory leak.
        /// </summary>
        /// <param name="pixelsPerUnit"> The pixels per unit for the returned sprite. </param>
        /// <returns> A <see cref="Sprite"/> from this image's pixel data. </returns>
        public Sprite GetSprite(float pixelsPerUnit)
        {
            Rect rect = new Rect(0.0f, 0.0f, Width, Height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        }

        /// <summary>
        /// Convert this image's pixel colors to a byte array. Each color channel will be represented by one byte.
        /// </summary>
        /// <param name="alpha"> Whether the output will have an alpha channel (RGBA or RGB). </param>
        /// <returns> This image's array of pixel colors as a <c>btye[]</c>.</returns>
        public byte[] ToByteArray(bool alpha = false)
        {
            int size = alpha ? Width * Height * 4 : Width * Height * 3; // Either four or three 8 bit channels.
            byte[] bytes = new byte[size];

            if (alpha)
            {
                for (int i = 0; i < width * height; ++i)
                {
                    bytes[4 * i + 0] = (byte)(pixels[i].r * 255.0f);
                    bytes[4 * i + 1] = (byte)(pixels[i].g * 255.0f);
                    bytes[4 * i + 2] = (byte)(pixels[i].b * 255.0f);
                    bytes[4 * i + 3] = (byte)(pixels[i].a * 255.0f);
                }
            }
            else 
            {
                for (int i = 0; i < width * height; ++i)
                {
                    bytes[3 * i + 0] = (byte)(pixels[i].r * 255.0f);
                    bytes[3 * i + 1] = (byte)(pixels[i].g * 255.0f);
                    bytes[3 * i + 2] = (byte)(pixels[i].b * 255.0f);
                }
            }
        

            return bytes;
        }

        /// <summary>
        /// Create a new image with its pixel colors determined from the <paramref name="bytes"/> byte array. Each byte
        /// represents one color channel. Three color channels make up an RGB color and four make an RGBA color. If the
        /// length of <paramref name="bytes"/> is not <c><paramref name="width"/> * <paramref name="height"/> * 3</c> for
        /// RGB colors or <c><paramref name="width"/> * <paramref name="height"/> * 4</c> for RGBA colors the function
        /// returns <c>null</c>.
        /// </summary>   
        /// <param name="width"> The width of the new image in pixels. </param>
        /// <param name="height"> The height of the new image in pixels. </param>
        /// <param name="bytes"> The byte array storing a flattened array of pixel colors. </param>
        /// <param name="alpha"> 
        /// Whether the specified colors are RGBA (with alpha channel) or RGB (without alpha channel).
        /// </param>
        public static RTImage FromByteArray(byte[] bytes, int width, int height, bool alpha = false)
        {
            // Make sure we got the right number of bytes, otherwise we return null.
            int size = alpha ? width * height * 4 : width * height * 3; // Either four or three 8 bit channels.
            if (bytes.Length != size)
                return null;

            Color[] pixels = new Color[width * height];

            if (alpha)
            {
                for (int i = 0; i < width * height; ++i)
                {
                    float r = bytes[4 * i + 0] / 255.0f;
                    float g = bytes[4 * i + 1] / 255.0f;
                    float b = bytes[4 * i + 2] / 255.0f;
                    float a = bytes[4 * i + 3] / 255.0f;
                    pixels[i] = new Color(r, g, b, a);
                }
            }
            else
            {
                for (int i = 0; i < width * height; ++i)
                {
                    float r = bytes[3 * i + 0] / 255.0f;
                    float g = bytes[3 * i + 1] / 255.0f;
                    float b = bytes[3 * i + 2] / 255.0f;
                    pixels[i] = new Color(r, g, b, 1.0f);
                }
            }

            return new RTImage(width, height, pixels);
        }

        /// <summary>
        /// Update the local Unity <see cref="Texture2D"/> version of this image. We keep track of this texture for use in
        /// the <see cref="GetSprite"/> function.
        /// </summary>
        private void UpdateTexture()
        {
            // Destroy the old texture to prevent a memory leak. Unity textures are apparently not garbage collected.
            if (texture != null)
                Object.Destroy(texture);

            texture = new Texture2D(Width, Height);
            texture.filterMode = FilterMode.Point;
            texture.SetPixels(Pixels);
            texture.Apply();
        }
    }
}
