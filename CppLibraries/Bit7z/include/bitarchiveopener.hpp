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

#ifndef BITARCHIVEOPENER_HPP
#define BITARCHIVEOPENER_HPP

#include <vector>
#include <map>

#include "../include/bitarchivehandler.hpp"
#include "../include/bitformat.hpp"
#include "../include/bittypes.hpp"

namespace bit7z {
    using std::vector;
    using std::map;
    using std::ostream;

    class BitInputArchive;

    /**
     * @brief Abstract class representing a generic archive opener.
     */
    class BitArchiveOpener : public BitArchiveHandler {
        public:

            /**
             * @return the archive format used by the archive opener.
             */
            const BitInFormat& format() const override;

            /**
             * @return the archive format used by the archive opener.
             */
            const BitInFormat& extractionFormat() const;

        protected:
            const BitInFormat& mFormat;

            BitArchiveOpener( const Bit7zLibrary& lib, const BitInFormat& format, const wstring& password = L"" );

            virtual ~BitArchiveOpener() override = 0;

            void extractToFileSystem( const BitInputArchive& in_archive,
                                      const wstring& in_file,
                                      const wstring& out_dir,
                                      const vector< uint32_t >& indices ) const;

            void extractToBuffer( const BitInputArchive& in_archive,
                                  vector< byte_t >& out_buffer,
                                  unsigned int index ) const;

            void extractToStream( const BitInputArchive& in_archive,
                                  ostream& out_stream,
                                  unsigned int index ) const;

            void extractToBufferMap( const BitInputArchive& in_archive,
                                     map< wstring, vector< byte_t > >& out_map ) const;
    };
}

#endif // BITARCHIVEOPENER_HPP
