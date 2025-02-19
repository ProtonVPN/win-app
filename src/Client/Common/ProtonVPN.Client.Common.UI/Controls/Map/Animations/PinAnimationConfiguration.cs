/*
 * Copyright (c) 2025 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;

namespace ProtonVPN.Client.Common.UI.Controls.Map.Animations;

public class PinAnimationConfiguration
{
    public required CircleAnimationValues TransparentCircle { get; init; }
    public required CircleAnimationValues NeutralCircle { get; init; }
    public required CircleAnimationValues CenterCircle { get; init; }

    public static PinAnimationConfiguration Get(PinAnimationType animationType)
    {
        return animationType switch
        {
            // NormalLocation: Disconnected -> OnHover
            PinAnimationType.NormalLocationDisconnectedToOnHover => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_HOVER
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_HOVER
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_HOVER
                }
            },

            // NormalLocation: OnHover -> Disconnected
            PinAnimationType.NormalLocationOnHoverToDisconnected => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_DEFAULT
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_DEFAULT
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_DEFAULT
                }
            },

            // OnHover -> Connecting
            PinAnimationType.OnHoverToConnecting => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING
                }
            },

            // NormalLocation: Connecting -> Disconnected
            PinAnimationType.NormalLocationConnectingToDisconnected => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_DEFAULT
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_DEFAULT
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_DEFAULT
                }
            },

            // NormalLocation: Connected -> Disconnected
            PinAnimationType.NormalLocationConnectedToDisconnected => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_DEFAULT
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_DEFAULT
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_DEFAULT
                }
            },

            // NormalLocation: Connected -> Connecting
            PinAnimationType.NormalLocationConnectedToConnecting => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING
                }
            },

            // CurrentLocation: Disconnected -> OnHover
            PinAnimationType.CurrentLocationDisconnectedToOnHover => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_DEFAULT
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_HOVER
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_HOVER
                }
            },

            // CurrentLocation: OnHover -> Disconnected
            PinAnimationType.CurrentLocationOnHoverToDisconnected => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE
                }
            },

            // CurrentLocation: Connecting -> Disconnected
            PinAnimationType.CurrentLocationConnectingToDisconnected => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE
                }
            },

            // CurrentLocation: Connected -> Connecting
            PinAnimationType.CurrentLocationConnectedToConnecting => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING
                }
            },

            // NormalLocation: Disconnected -> Connecting
            PinAnimationType.NormalLocationDisconnectedToConnecting => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING
                }
            },

            // CurrentLocation: Disconnected -> Connecting
            PinAnimationType.CurrentLocationDisconnectedToConnecting => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING
                }
            },

            // Connected -> OnHover
            PinAnimationType.ConnectedToOnHover => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_HOVER
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_HOVER
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_HOVER
                }
            },

            // OnHover -> Connected
            PinAnimationType.OnHoverToConnected => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_HOVER,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE
                }
            },

            // Connecting -> Connected
            PinAnimationType.ConnectingToConnected => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_CONNECTING,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE
                }
            },

            // Disconnected -> CurrentLocation
            PinAnimationType.DisconnectedToCurrentLocation => new PinAnimationConfiguration
            {
                TransparentCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE
                },
                NeutralCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE
                },
                CenterCircle = new CircleAnimationValues
                {
                    Start = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_DEFAULT,
                    End = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE
                }
            },

            _ => throw new ArgumentOutOfRangeException(nameof(animationType), $"Unhandled pin animation type: {animationType}")
        };
    }
}