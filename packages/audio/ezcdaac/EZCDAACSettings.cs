// ****************************************************************************
// 
// Copyright (C) 2005-2026 Doom9 & al
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// ****************************************************************************

using System;

namespace MeGUI
{
    public enum EzcdAacProfile
    {
        [EnumTitle("xHE-AAC")]
        XHE,
        [EnumTitle("AAC-LC")]
        AACLC,
        [EnumTitle("HE-AAC")]
        HE,
        [EnumTitle("HE-AACv2")]
        HEV2
    }

    public enum EzcdAacMode
    {
        [EnumTitle("VBR")]
        VBR,
        [EnumTitle("CBR")]
        CBR
    }

    /// <summary>
    /// settings of the EZCD-AAC encoder (ezcd-aac.exe)
    /// </summary>
    public class EZCDAACSettings : AudioCodecSettings
    {
        public static readonly string ID = "EZCDAAC";

        public static readonly object[] SupportedBitrates = new object[] { 16, 24, 32, 48, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384, 512 };
        public static readonly EzcdAacProfile[] SupportedProfiles = new EzcdAacProfile[] { EzcdAacProfile.XHE, EzcdAacProfile.AACLC, EzcdAacProfile.HE, EzcdAacProfile.HEV2 };
        public static readonly EzcdAacMode[] SupportedModes = new EzcdAacMode[] { EzcdAacMode.VBR, EzcdAacMode.CBR };

        /// <summary>
        /// lowest / highest VBR quality value supported by the encoder
        /// </summary>
        public const int MinimumQuality = 0;
        public const int MaximumQuality = 6;
        /// <summary>
        /// lowest CBR bitrate (kbit/s) offered in the UI
        /// </summary>
        public const int MinimumBitrate = 16;
        /// <summary>
        /// highest CBR bitrate (kbit/s) offered in the UI
        /// </summary>
        public const int MaximumBitrate = 512;

        /// <summary>
        /// highest CBR bitrate the encoder actually accepts for a profile.
        /// the AAC back-end (encm_aacf) gives up much earlier than the xHE-AAC
        /// one and refuses to initialize instead of clamping:
        ///   HE-AACv2 : 64 kbit/s (stereo only)
        ///   HE-AAC   : 64 kbit/s mono / 128 kbit/s stereo
        ///   xHE-AAC  : 384 kbit/s at 44.1/48 kHz
        /// </summary>
        public static int GetMaximumBitrate(EzcdAacProfile eProfile)
        {
            switch (eProfile)
            {
                case EzcdAacProfile.HEV2: return 64;
                case EzcdAacProfile.HE: return 128;
                case EzcdAacProfile.XHE: return 384;
                default: return MaximumBitrate;
            }
        }

        public EZCDAACSettings()
            : base(ID, AudioCodec.AAC, AudioEncoderType.EZCDAAC, 128, BitrateManagementMode.VBR)
        {
            // the encoder only accepts mono or stereo input
            base.DownmixMode = ChannelMode.StereoDownmix;
            base.supportedBitrates = Array.ConvertAll<object, int>(SupportedBitrates, delegate (object o) { return (int)o; });
            mode = EzcdAacMode.VBR;
            profile = EzcdAacProfile.XHE;
            quality = 3;
        }

        private EzcdAacProfile profile;
        public EzcdAacProfile Profile
        {
            get { return profile; }
            set
            {
                profile = value;
                // VBR ( -v ) is only available for xHE-AAC: the -v presets of the
                // AAC-LC / HE-AAC / HE-AACv2 encoder already select the profile
                if (profile != EzcdAacProfile.XHE)
                    mode = EzcdAacMode.CBR;
            }
        }

        private EzcdAacMode mode;
        public EzcdAacMode Mode
        {
            get { return mode; }
            set
            {
                if (value == EzcdAacMode.VBR && profile != EzcdAacProfile.XHE)
                    return;
                mode = value;
            }
        }

        private int quality;
        public int Quality
        {
            get { return quality; }
            set
            {
                if (value < MinimumQuality)
                    quality = MinimumQuality;
                else if (value > MaximumQuality)
                    quality = MaximumQuality;
                else
                    quality = value;
            }
        }

        public override BitrateManagementMode BitrateMode
        {
            get
            {
                return mode == EzcdAacMode.CBR ? BitrateManagementMode.CBR : BitrateManagementMode.VBR;
            }
            set
            {
                Mode = (value == BitrateManagementMode.CBR) ? EzcdAacMode.CBR : EzcdAacMode.VBR;
            }
        }

        /// <summary>
        /// HE-AACv2 can only handle stereo input
        /// </summary>
        public bool IsStereoRequired
        {
            get { return profile == EzcdAacProfile.HEV2; }
        }

        /// <summary>
        /// highest CBR bitrate valid for the selected profile
        /// </summary>
        public int MaximumBitrateForProfile
        {
            get { return GetMaximumBitrate(profile); }
        }

        /// <summary>
        /// keeps the CBR bitrate inside the range the selected profile accepts
        /// </summary>
        public override int Bitrate
        {
            get { return base.Bitrate; }
            set { base.Bitrate = Math.Min(value, MaximumBitrateForProfile); }
        }
    }
}
