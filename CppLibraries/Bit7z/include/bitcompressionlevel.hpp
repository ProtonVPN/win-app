/*
 * bit7z - A C++ static library to interface with the 7-zip DLLs.
 * Copyright (c) 2014-2019  Riccardo Ostani - All Rights Reserved.
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * Bit7z is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with bit7z; if not, see https://www.gnu.org/licenses/.
 */

#ifndef BITCOMPRESSIONLEVEL_HPP
#define BITCOMPRESSIONLEVEL_HPP

namespace bit7z {
    /**
     * @brief The BitCompressionLevel enum represents the compression level used by 7z when creating archives.
     * @note It uses the same values as in the 7z SDK (https://sevenzip.osdn.jp/chm/cmdline/switches/method.htm#ZipX).
     */
    enum class BitCompressionLevel {
        NONE = 0,    ///< Copy mode (no compression)
        FASTEST = 1, ///< Fastest compressing
        FAST = 3,    ///< Fast compressing
        NORMAL = 5,  ///< Normal compressing
        MAX = 7,     ///< Maximum compressing
        ULTRA = 9    ///< Ultra compressing
    };
}

#endif // BITCOMPRESSIONLEVEL_HPP
