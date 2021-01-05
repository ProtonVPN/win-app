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

#ifndef BITARCHIVEINFO_HPP
#define BITARCHIVEINFO_HPP

#include "../include/bitarchiveopener.hpp"
#include "../include/bitinputarchive.hpp"
#include "../include/bitarchiveitem.hpp"
#include "../include/bittypes.hpp"

struct IInArchive;
struct IOutArchive;
struct IArchiveExtractCallback;

namespace bit7z {
    /**
     * @brief The BitArchiveInfo class allows to retrieve metadata information of archives and their content.
     */
    class BitArchiveInfo : public BitArchiveOpener, public BitInputArchive {
        public:
            /**
             * @brief Constructs a BitArchiveInfo object, opening the input file archive.
             *
             * @note When bit7z is compiled using the BIT7Z_AUTO_FORMAT macro define, the format
             * argument has default value BitFormat::Auto (automatic format detection of the input archive).
             * On the other hand, when BIT7Z_AUTO_FORMAT is not defined (i.e. no auto format detection available)
             * the format argument must be specified.
             *
             * @param lib       the 7z library used.
             * @param in_file   the input archive file path.
             * @param format    the input archive format.
             * @param password  the password needed to open the input archive.
             */
            BitArchiveInfo( const Bit7zLibrary& lib,
                            const wstring& in_file,
                            const BitInFormat& format DEFAULT_FORMAT,
                            const wstring& password = L"" );

            /**
             * @brief Constructs a BitArchiveInfo object, opening the archive in the input buffer.
             *
             * @note When bit7z is compiled using the BIT7Z_AUTO_FORMAT macro define, the format
             * argument has default value BitFormat::Auto (automatic format detection of the input archive).
             * On the other hand, when BIT7Z_AUTO_FORMAT is not defined (i.e. no auto format detection available)
             * the format argument must be specified.
             *
             * @param lib       the 7z library used.
             * @param in_buffer the input buffer containing the archive.
             * @param format    the input archive format.
             * @param password  the password needed to open the input archive.
             */
            BitArchiveInfo( const Bit7zLibrary& lib,
                            const vector< byte_t >& in_buffer,
                            const BitInFormat& format DEFAULT_FORMAT,
                            const wstring& password = L"" );

            /**
             * @brief Constructs a BitArchiveInfo object, opening the archive from the standard input stream.
             *
             * @note When bit7z is compiled using the BIT7Z_AUTO_FORMAT macro define, the format
             * argument has default value BitFormat::Auto (automatic format detection of the input archive).
             * On the other hand, when BIT7Z_AUTO_FORMAT is not defined (i.e. no auto format detection available)
             * the format argument must be specified.
             *
             * @param lib       the 7z library used.
             * @param in_stream the standard input stream of the archive.
             * @param format    the input archive format.
             * @param password  the password needed to open the input archive.
             */
            BitArchiveInfo( const Bit7zLibrary& lib,
                            std::istream& in_stream,
                            const BitInFormat& format DEFAULT_FORMAT,
                            const wstring& password = L"" );

            /**
             * @brief BitArchiveInfo destructor.
             *
             * @note It releases the input archive file.
             */
            virtual ~BitArchiveInfo() override;

            /**
             * @return a map of all the available (i.e. non empty) archive properties and their respective values.
             */
            map< BitProperty, BitPropVariant > archiveProperties() const;

            /**
             * @return a vector of all the archive items as BitArchiveItem objects.
             */
            vector< BitArchiveItem > items() const;

            /**
             * @return the number of folders contained in the archive.
             */
            uint32_t foldersCount() const;

            /**
             * @return the number of files contained in the archive.
             */
            uint32_t filesCount() const;

            /**
             * @return the total uncompressed size of the archive content.
             */
            uint64_t size() const;

            /**
             * @return the total compressed size of the archive content.
             */
            uint64_t packSize() const;

            /**
             * @return true if and only if the archive has at least one encrypted item.
             */
            bool hasEncryptedItems() const;

            /**
             * @return the number of volumes composing the archive.
             */
            uint32_t volumesCount() const;

            /**
             * @return true if and only if the archive is composed by multiple volumes.
             */
            bool isMultiVolume() const;

            /**
             * @return true if and only if the archive was created using solid compression.
             */
            bool isSolid() const;
    };
}

#endif // BITARCHIVEINFO_HPP
