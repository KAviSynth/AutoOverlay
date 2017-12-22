﻿using System;
using System.Drawing;
using System.Windows.Forms;
using AvsFilterNet;

namespace AutoOverlay
{
    public abstract class OverlayFilter : AvisynthFilter
    {
        protected dynamic DynamicEnv => DynamicEnviroment.Env;
        protected ScriptEnvironment StaticEnv => DynamicEnviroment.Env;

        public sealed override void Initialize(AVSValue args, ScriptEnvironment env)
        {
            using (new DynamicEnviroment(env))
                Initialize(args);
            base.Initialize(args, env);
        }

        protected virtual void Initialize(AVSValue args)
        {
            var vi = new VideoInfo
            {
                width = 800,
                height = 400,
                pixel_type = ColorSpaces.CS_BGR32,
                fps_numerator = 25,
                fps_denominator = 1,
                num_frames = 1
            };
            SetVideoInfo(ref vi);
        }
        
        public sealed override VideoFrame GetFrame(int n, ScriptEnvironment env)
        {
            using (new DynamicEnviroment(env))
            {
                return GetFrame(n);
            }
        }

        protected virtual VideoFrame GetFrame(int n)
        {
            var blank = DynamicEnv.BlankClip(width: GetVideoInfo().width, height: GetVideoInfo().height);
            var subtitled = blank.Subtitle(ToString().Replace("\n", "\\n"), align: 8, lsp: 0, size: 30);
            return subtitled[0];
        }

        protected Clip GetBlankClip(Clip clip, bool white)
        {
            if (clip.GetVideoInfo().pixel_type.HasFlag(ColorSpaces.CS_PLANAR | ColorSpaces.CS_INTERLEAVED))
                return DynamicEnv.BlankClip(clip, color_yuv: white ? 0xFF0000 : 0x000000);
            if (clip.GetVideoInfo().pixel_type.HasFlag(ColorSpaces.CS_PLANAR))
                return DynamicEnv.BlankClip(clip, color_yuv: white ? 0xFF8080 : 0x008080);
            return DynamicEnv.BlankClip(clip, color: white ? 0xFFFFFF : 0);
        }

        protected dynamic ResizeRotate(
            Clip clip, 
            string resizeFunc, string rotateFunc, 
            int width, int height, int angle = 0, 
            RectangleF crop = default(RectangleF))
        {
            if (clip == null || crop == RectangleF.Empty && width == clip.GetVideoInfo().width && height == clip.GetVideoInfo().height)
                return clip;
            dynamic resized;
            if (crop == RectangleF.Empty)
                resized = clip.Dynamic().Invoke(resizeFunc, width, height);
            else resized = clip.Dynamic().Invoke(resizeFunc, width, height, crop.Left, crop.Top, -crop.Right, -crop.Bottom);
            if (angle == 0)
                return resized;
            return resized.Invoke(rotateFunc, angle / 100.0);
        }
    }
}