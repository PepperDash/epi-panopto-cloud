// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Interfaces
{
    /// <summary>
    /// Interface to define Camera Control.
    /// </summary>
    public interface ICameraControl
    {
        /// <summary>
        /// Flag to indicate that this camera supports Pan and Tilt.
        /// </summary>
        bool SupportsPanAndTilt { get; }
        /// <summary>
        /// Method to move the camera Left.
        /// </summary>
        void Left();
        /// <summary>
        /// Method to move the camera Right.
        /// </summary>
        void Right();
        /// <summary>
        /// Method to move the camera Up.
        /// </summary>
        void Up();
        /// <summary>
        /// Method to move the camera Down.
        /// </summary>
        void Down();

        /// <summary>
        /// Flag to indicate that this camera supports Zoom.
        /// </summary>
        bool SupportsZoom { get; }
        /// <summary>
        /// Method to have the camera Zoom In.
        /// </summary>
        void ZoomIn();
        /// <summary>
        /// Method to have the camera Zoom Out.
        /// </summary>
        void ZoomOut();
        /// <summary>
        /// Method to have the camera stop its current action.
        /// </summary>
        void Stop();

        /// <summary>
        /// Flag to indicate that this camera supports manual focus control.
        /// </summary>
        bool SupportsManualFocus { get; }
        /// <summary>
        /// Flag to indicate that this camera supports auto focus.
        /// </summary>
        bool SupportsAutoFocus { get; }

        /// <summary>
        /// Flag to indicate that this camera is in Auto Focus Mode.
        /// </summary>
        bool InAutoFocusFeedback { get; }
        /// <summary>
        /// Method to have the camera focus in.
        /// </summary>
        void FocusIn();
        /// <summary>
        /// Method to have the camera focus out.
        /// </summary>
        void FocusOut();
        /// <summary>
        /// Method to enable auto focus for a device.
        /// </summary>
        void EnableAutoFocus();
        /// <summary>
        /// Method to reset the camera to its default values for pan and tilt.
        /// </summary>
        void ResetPosition();
    }
}
