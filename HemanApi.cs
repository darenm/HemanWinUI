using System;
using System.IO;
using System.Runtime.InteropServices;

using CC = System.Runtime.InteropServices.CallingConvention;

namespace HemanWinUI
{
    internal class HemanApi
    {
        const int Size = 512;

        static readonly int[] cpLocations =
            {
                000,  // Dark Blue
                126,  // Light Blue
                127,  // Yellow
                128,  // Dark Green
                160,  // Brown
                200,  // White
                255,  // White
            };

        static readonly uint[] cpColors =
            {
                0x001070,  // Dark Blue
                0x2C5A7C,  // Light Blue
                0xE0F0A0,  // Yellow
                0x5D943C,  // Dark Green
                0x606011,  // Brown
                0xFFFFFF,  // White
                0xFFFFFF,  // White            
            };

        static readonly float[] lightPos = { -0.5f, 0.5f, 1.0f };

        public static int GetNumberOfThreads()
        {
            return NativeMethods.heman_get_num_threads();
        }

        public static string RenderExample()
        {
            IntPtr[] frames = new IntPtr[5];

            GCHandle cpLocationsHandle = GCHandle.Alloc(cpLocations, GCHandleType.Pinned);
            GCHandle cpColorsHandle = GCHandle.Alloc(cpColors, GCHandleType.Pinned);
            GCHandle lightPosHandle = GCHandle.Alloc(lightPos, GCHandleType.Pinned);
            GCHandle framesHandle = GCHandle.Alloc(frames, GCHandleType.Pinned);

            try
            {
                IntPtr cpLocationsPointer = cpLocationsHandle.AddrOfPinnedObject();
                IntPtr cpColorsPointer = cpColorsHandle.AddrOfPinnedObject();
                IntPtr lightPosPointer = lightPosHandle.AddrOfPinnedObject();
                IntPtr framesPointer = framesHandle.AddrOfPinnedObject();

                // Create a gradient
                var grad = NativeMethods.heman_color_create_gradient(256, cpColors.Length, cpLocationsPointer, cpColorsPointer);

                // Generate the heightmap.
                var time = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).Seconds;
                var hmap = NativeMethods.heman_generate_island_heightmap(Size, Size, time);
                var hmapViz = NativeMethods.heman_ops_normalize_f32(hmap, -0.5f, 0.5f);

                // Compute ambient occlusion.
                var occ = NativeMethods.heman_lighting_compute_occlusion(hmap);

                // Create a normal map.
                var norm = NativeMethods.heman_lighting_compute_normals(hmap);
                var normviz = NativeMethods.heman_ops_normalize_f32(norm, -1, 1);

                // Create an albedo image.
                var albedo = NativeMethods.heman_color_apply_gradient(hmap, -0.5f, 0.5f, grad);
                NativeMethods.heman_image_destroy(grad);

                // Perform lighting.
                var final = NativeMethods.heman_lighting_apply(hmap, albedo, 1, 1, 0.5f, lightPosPointer);

                // Create film strip image
                frames[0] = NativeMethods.heman_color_from_grayscale(hmapViz);
                frames[1] = NativeMethods.heman_color_from_grayscale(occ);
                frames[2] = normviz;
                frames[3] = albedo;
                frames[4] = final;


                var filmstrip = NativeMethods.heman_ops_stitch_horizontal(framesPointer, frames.Length);

                var path = Path.GetTempPath();
                var randomFilename = Path.GetRandomFileName();
                var fileName = Path.Combine(path, randomFilename.Split('.')[0] + ".png");

                NativeMethods.hut_write_image(fileName, filmstrip, 0f, 1f);

                // Copy the final IntPtr data to a HemanImage struct
                var finalImage = Marshal.PtrToStructure<HemanImage>(final);
                var width = finalImage.Width;

                // Cleanup
                NativeMethods.heman_image_destroy(frames[0]);
                NativeMethods.heman_image_destroy(frames[1]);
                NativeMethods.heman_image_destroy(hmap);
                NativeMethods.heman_image_destroy(hmapViz);
                NativeMethods.heman_image_destroy(occ);
                NativeMethods.heman_image_destroy(norm);
                NativeMethods.heman_image_destroy(normviz);
                NativeMethods.heman_image_destroy(albedo);
                NativeMethods.heman_image_destroy(final);

                return fileName;
            }
            finally
            {
                FreeHandle(cpLocationsHandle);
                FreeHandle(cpColorsHandle);
                FreeHandle(lightPosHandle);
                FreeHandle(framesHandle);
            }
        }

        private static void FreeHandle(GCHandle handle)
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }


        private static class NativeMethods
        {
            const string DLL = "heman.dll";

            [DllImport(DLL)]
            internal static extern int heman_get_num_threads();

            [DllImport(DLL)]
            internal static extern IntPtr heman_color_create_gradient(int width, int num_colors, IntPtr cp_locations, IntPtr cp_colors);

            [DllImport(DLL)]
            internal static extern IntPtr heman_generate_island_heightmap(int width, int height, int seed);

            [DllImport(DLL)]
            internal static extern IntPtr heman_ops_normalize_f32(IntPtr source, float minval, float maxval);

            [DllImport(DLL)]
            internal static extern IntPtr heman_lighting_compute_occlusion(IntPtr heightmap);

            [DllImport(DLL)]
            internal static extern IntPtr heman_lighting_compute_normals(IntPtr heightmap);
            [DllImport(DLL)]
            internal static extern IntPtr heman_color_apply_gradient(IntPtr heightmap, float minheight, float maxheight, IntPtr gradient);

            [DllImport(DLL)]
            internal static extern void heman_image_destroy(IntPtr img);

            [DllImport(DLL)]
            internal static extern IntPtr heman_lighting_apply(IntPtr heightmap, IntPtr colorbuffer, float occlusion, float diffuse, float diffuse_softening, IntPtr light_position);

            [DllImport(DLL)]
            internal static extern IntPtr heman_color_from_grayscale(IntPtr gray);

            [DllImport(DLL)]
            internal static extern IntPtr heman_ops_stitch_horizontal(IntPtr images, int count);

            [DllImport(DLL, CharSet = CharSet.Ansi)]
            internal static extern void hut_write_image(string filename, IntPtr img, float minv, float maxv);

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HemanImage
        {
            public int Width;
            public int Height;
            public int NBands;
            public IntPtr data; // Array of floats which we don't need to access
        }
    }
}
