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

#ifndef BITEXTRACTOR_HPP
#define BITEXTRACTOR_HPP

#include "../include/bitarchiveopener.hpp"
#include "../include/bittypes.hpp"

struct IInArchive;

namespace bit7z {
    class BitInputArchive;

    /**
     * @brief The BitExtractor class allows to extract the content of file archives.
     */
    class BitExtractor : public BitArchiveOpener {
        public:
            /**
             * @brief Constructs a BitExtractor object.
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
            explicit BitExtractor( const Bit7zLibrary& lib, const BitInFormat& format DEFAULT_FORMAT );

            /**
             * @brief Extracts the given archive into the choosen directory.
             *
             * @param in_file       the input archive file.
             * @param out_dir       the output directory where extracted files will be put.
             */
            void extract( const wstring& in_file, const wstring& out_dir = L"" ) const;

            /**
             * @brief Extracts the wildcard matching files in the given archive into the choosen directory.
             *
             * @param in_file       the input archive file.
             * @param item_filter   the wildcard pattern used for matching the paths of files inside the archive.
             * @param out_dir       the output directory where extracted files will be put.
             */
            void extractMatching( const wstring& in_file,
                                  const wstring& item_filter,
                                  const wstring& out_dir = L"" ) const;

#ifdef BIT7Z_REGEX_MATCHING
            /**
             * @brief Extracts the regex matching files in the given archive into the choosen directory.
             *
             * @note Available only when compiling bit7z using the BIT7Z_REGEX_MATCHING preprocessor define.
             *
             * @param in_file       the input archive file.
             * @param regex         the regex used for matching the paths of files inside the archive.
             * @param out_dir       the output directory where extracted files will be put.
             */
            void extractMatchingRegex( const wstring& in_file, const wstring& regex, const wstring& out_dir ) const;
#endif

            /**
             * @brief Extracts the specified items in the given archive into the choosen directory.
             *
             * @param in_file   the input archive file.
             * @param out_dir   the output directory where extracted files will be put.
             * @param indices   the array of indices of the files in the archive that must be extracted.
             */
            void extractItems( const wstring& in_file,
                               const vector< uint32_t >& indices,
                               const wstring& out_dir = L"" ) const;

            /**
             * @brief Extracts a file from the given archive into the output buffer.
             *
             * @param in_file      the input archive file.
             * @param out_buffer   the output buffer where the content of the archive will be put.
             * @param index        the index of the file to be extracted from in_file.
             */
            void extract( const wstring& in_file, vector< byte_t >& out_buffer, unsigned int index = 0 ) const;


            /**
             * @brief Extracts a file from the given archive into the output stream.
             *
             * @param in_file      the input archive file.
             * @param out_stream   the (binary) stream where the content of the archive will be put.
             * @param index        the index of the file to be extracted from in_file.
             */
            void extract( const wstring& in_file, ostream& out_stream, unsigned int index = 0 ) const;

            /**
             * @brief Extracts the content of the given archive into a map of memory buffers, where keys are the paths
             * of the files (inside the archive) and values are the corresponding decompressed contents.
             *
             * @param in_file   the input archive file.
             * @param out_map   the output map.
             */
            void extract( const wstring& in_file, map< wstring, vector< byte_t > >& out_map ) const;

            /**
             * @brief Tests the given archive without extracting its content.
             *
             * If the input archive is not valid, a BitException is thrown!
             *
             * @param in_file   the input archive file to be tested.
             */
            void test( const wstring& in_file ) const;

        private:
            void extractMatchingFilter( const wstring& in_file,
                                        const wstring& out_dir,
                                        const function< bool( const wstring& ) >& filter ) const;
    };
}
#endif // BITEXTRACTOR_HPP
