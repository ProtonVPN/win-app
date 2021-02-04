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

#ifndef BITMEMEXTRACTOR_HPP
#define BITMEMEXTRACTOR_HPP

#include "../include/bitarchiveopener.hpp"
#include "../include/bittypes.hpp"

namespace bit7z {
    /**
     * @brief The BitMemExtractor class allows to extract the content of in-memory archives.
     */
    class BitMemExtractor : public BitArchiveOpener {
        public:
            /**
             * @brief Constructs a BitMemExtractor object.
             *
             * The Bit7zLibrary parameter is needed in order to have access to the functionalities
             * of the 7z DLLs. On the other hand, the BitInFormat is required in order to know the
             * format of the input archive.
             *
             * @note When bit7z is compiled using the BIT7Z_AUTO_FORMAT macro define, the format
             * argument has default value BitFormat::Auto (automatic format detection of the input archive).
             * On the other hand, when BIT7Z_AUTO_FORMAT is not defined (i.e. no auto format detection available)
             * the format argument must be specified.
             *
             * @param lib       the 7z library used.
             * @param format    the input archive format.
             */
            explicit BitMemExtractor( const Bit7zLibrary& lib, const BitInFormat& format DEFAULT_FORMAT );

            /**
             * @brief Extracts the given buffer archive into the choosen directory.
             *
             * @param in_buffer     the buffer containing the archive to be extracted.
             * @param out_dir       the output directory where to put the file extracted.
             */
            void extract( const vector< byte_t >& in_buffer, const wstring& out_dir = L"" ) const;

            /**
             * @brief Extracts the given buffer archive into the output buffer.
             *
             * @param in_buffer    the buffer containing the archive to be extracted.
             * @param out_buffer   the output buffer where the content of the archive will be put.
             * @param index        the index of the file to be extracted from in_buffer.
             */
            void extract( const vector< byte_t >& in_buffer,
                          vector< byte_t >& out_buffer,
                          unsigned int index = 0 ) const;

            /**
             * @brief Extracts the given buffer archive into the output standard stream.
             *
             * @param in_buffer    the buffer containing the archive to be extracted.
             * @param out_stream   the (binary) stream where the content of the archive will be put.
             * @param index        the index of the file to be extracted from in_buffer.
             */
            void extract( const vector< byte_t >& in_buffer, ostream& out_stream, unsigned int index = 0 ) const;

            /**
             * @brief Extracts the given buffer archive into a map of memory buffers, where keys are the paths
             * of the files (inside the archive) and values are the corresponding decompressed contents.
             *
             * @param in_buffer    the buffer containing the archive to be extracted.
             * @param out_map      the output map.
             */
            void extract( const vector< byte_t >& in_buffer, map< wstring, vector< byte_t > >& out_map ) const;

            /**
             * @brief Tests the given buffer archive without extracting its content.
             *
             * If the input archive is not valid, a BitException is thrown.
             *
             * @param in_buffer    the buffer containing the archive to be tested.
             */
            void test( const vector< byte_t >& in_buffer ) const;
    };
}

#endif // BITMEMEXTRACTOR_HPP
