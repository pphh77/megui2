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
using System.Windows.Forms;

namespace MeGUI.packages.audio.ezcdaac
{
    public partial class EZCDAACConfigurationPanel : MeGUI.core.details.audio.AudioConfigurationPanel, Editable<EZCDAACSettings>
    {
        public EZCDAACConfigurationPanel()
            : base()
        {
            InitializeComponent();
            cbProfile.Items.AddRange(EnumProxy.CreateArray(EZCDAACSettings.SupportedProfiles));
            cbMode.Items.AddRange(EnumProxy.CreateArray(EZCDAACSettings.SupportedModes));
        }

        #region properties
        private int bitrate = 128;
        private int quality = 3;
        /// <summary>
        /// used to suppress the slider handler while the slider range is adjusted
        /// </summary>
        private bool _bUpdatingTrackBar = false;

        /// <summary>
        /// gets / sets the settings that are being shown in this configuration dialog
        /// </summary>
        protected override AudioCodecSettings CodecSettings
        {
            get
            {
                EZCDAACSettings oSettings = new EZCDAACSettings();
                // setting the profile may force CBR for the AAC-LC / HE-AAC / HE-AACv2 encoders
                oSettings.Profile = (EzcdAacProfile)(cbProfile.SelectedItem as EnumProxy).RealValue;
                oSettings.Mode = (EzcdAacMode)(cbMode.SelectedItem as EnumProxy).RealValue;
                if (oSettings.Mode == EzcdAacMode.CBR)
                    oSettings.Bitrate = trackBar.Value;
                else
                    oSettings.Quality = trackBar.Value;
                return oSettings;
            }
            set
            {
                EZCDAACSettings oSettings = value as EZCDAACSettings;
                bitrate = oSettings.Bitrate;
                quality = oSettings.Quality;
                cbProfile.SelectedItem = EnumProxy.Create(oSettings.Profile);
                cbMode.SelectedItem = EnumProxy.Create(oSettings.Mode);
                updateModeAvailability();
                updateTrackBar();
            }
        }

        EZCDAACSettings Editable<EZCDAACSettings>.Settings
        {
            get
            {
                return (EZCDAACSettings)Settings;
            }
            set
            {
                Settings = value;
            }
        }
        #endregion

        #region helper
        private EzcdAacMode SelectedMode
        {
            get
            {
                if (cbMode.SelectedItem == null)
                    return EzcdAacMode.VBR;
                return (EzcdAacMode)(cbMode.SelectedItem as EnumProxy).RealValue;
            }
        }

        private void updateTrackBar()
        {
            _bUpdatingTrackBar = true;
            try
            {
                if (SelectedMode == EzcdAacMode.CBR)
                {
                    trackBar.Minimum = EZCDAACSettings.MinimumBitrate;
                    trackBar.Maximum = EZCDAACSettings.MaximumBitrate;
                    trackBar.TickFrequency = EZCDAACSettings.MaximumBitrate / 16;
                    trackBar.Value = Math.Max(Math.Min(bitrate, trackBar.Maximum), trackBar.Minimum);
                }
                else
                {
                    trackBar.Minimum = EZCDAACSettings.MinimumQuality;
                    trackBar.Maximum = EZCDAACSettings.MaximumQuality;
                    trackBar.TickFrequency = 1;
                    trackBar.Value = Math.Max(Math.Min(quality, trackBar.Maximum), trackBar.Minimum);
                }
            }
            finally
            {
                _bUpdatingTrackBar = false;
            }
            updateTextBox();
        }

        /// <summary>
        /// VBR (quality slider) is only offered by the xHE-AAC encoder
        /// </summary>
        private void updateModeAvailability()
        {
            if (cbProfile.SelectedItem == null || cbMode.SelectedItem == null)
                return;

            bool bVBR = (EzcdAacProfile)(cbProfile.SelectedItem as EnumProxy).RealValue == EzcdAacProfile.XHE;
            cbMode.Enabled = bVBR;
            if (!bVBR && SelectedMode == EzcdAacMode.VBR)
                cbMode.SelectedItem = EnumProxy.Create(EzcdAacMode.CBR);
        }

        private void updateTextBox()
        {
            if (cbMode.SelectedItem == null || cbProfile.SelectedItem == null)
                return;

            string strProfile = (cbProfile.SelectedItem as EnumProxy).ToString();
            if (SelectedMode == EzcdAacMode.CBR)
                encoderGroupBox.Text = String.Format(" EZCD-AAC - {0} - Constant Bitrate @ {1} kbit/s ", strProfile, trackBar.Value);
            else
                encoderGroupBox.Text = String.Format(" EZCD-AAC - {0} - Variable Bitrate @ Quality = {1} ", strProfile, trackBar.Value);
        }
        #endregion

        #region events
        private void cbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateTrackBar();
        }

        private void cbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateModeAvailability();
            updateTrackBar();
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (_bUpdatingTrackBar)
                return;

            if (SelectedMode == EzcdAacMode.CBR)
                bitrate = trackBar.Value;
            else
                quality = trackBar.Value;

            updateTextBox();
        }
        #endregion
    }
}
