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

#ifndef BITFORMAT_HPP
#define BITFORMAT_HPP

#include <bitset>
#include <string>

#include "../include/bitguids.hpp"
#include "../include/bitcompressionmethod.hpp"

#define FEATURES_COUNT 7

namespace bit7z {
    using std::wstring;
    typedef std::bitset< FEATURES_COUNT > FeaturesSet; //MSVC++11 (VS 2012) does not support new 'using' keyword usage!

    /**
     * @brief The FormatFeatures enum specifies the features supported by an archive file format.
     */
    enum FormatFeatures : unsigned {
        MULTIPLE_FILES    = 1 << 0,///< The format can compress/extract multiple files         (2^0 = 0000001)
        SOLID_ARCHIVE     = 1 << 1,///< The format supports solid archives                     (2^1 = 0000010)
        COMPRESSION_LEVEL = 1 << 2,///< The format is able to use different compression levels (2^2 = 0000100)
        ENCRYPTION        = 1 << 3,///< The format supports archive encryption                 (2^3 = 0001000)
        HEADER_ENCRYPTION = 1 << 4,///< The format can encrypt the file names                  (2^4 = 0010000)
        INMEM_COMPRESSION = 1 << 5,///< The format is able to create archives in-memory        (2^5 = 0100000)
        MULTIPLE_METHODS  = 1 << 6 ///< The format can use different compression methods       (2^6 = 1000000)
    };

    /**
     * @brief The BitInFormat class specifies an extractable archive format.
     *
     * @note Usually, the user of the library should not create new formats and, instead,
     * use the ones provided by the BitFormat namespace.
     */
    class BitInFormat {
        public:
            /**
             * @brief Constructs a BitInFormat object with the id value used by the 7z SDK.
             * @param value  the value of the format in the 7z SDK.
             */
            explicit BitInFormat( unsigned char value );

            /**
             * @return the value of the format in the 7z SDK.
             */
            int value() const;

            /**
             * @return the GUID that identifies the file format in the 7z SDK.
             */
            const GUID guid() const;

            /**
             * @param other  the target object to compare to.
             * @return true if this format is equal to "other".
             */
            bool operator==( BitInFormat const& other ) const;

            /**
             * @param other  the target object to compare to.
             * @return true if this format is not equal to "other".
             */
            bool operator!=( BitInFormat const& other ) const;

        private:
            const unsigned char mValue;

            //non-copyable
            BitInFormat( const BitInFormat& other );

            BitInFormat& operator=( const BitInFormat& other );

            //non-movable
            BitInFormat( BitInFormat&& other );

            BitInFormat& operator=( BitInFormat&& other );
    };

    /**
     * @brief The BitInOutFormat class specifies a format available for creating new archives and extract old ones
     *
     * @note Usually, the user of the library should not create new formats and, instead,
     * use the ones provided by the BitFormat namespace
     */
    class BitInOutFormat : public BitInFormat {
        public:
            /**
             * @brief Constructs a BitInOutFormat object with a id value, an extension and a set of supported features
             *
             * @param value         the value of the format in the 7z SDK
             * @param ext           the default file extension of the archive format
             * @param defaultMethod the default compression method of the archive format.
             * @param features      the set of features supported by the archive format
             */
            BitInOutFormat( unsigned char value,
                            const wstring& ext,
                            BitCompressionMethod defaultMethod,
                            FeaturesSet features );

            /**
             * @return the default file estension of the archive format
             */
            const wstring& extension() const;

            /**
             * @return the bitset of the features supported by the format
             */
            const FeaturesSet features() const;

            /**
             * @brief Checks if the format has a specific feature (see FormatFeatures enum)
             * @param feature   feature to be checked
             * @return true if the format has the feature, false otherwise
             */
            bool hasFeature( FormatFeatures feature ) const;

            /**
             * @return the default compression method of the archive format.
             */
            BitCompressionMethod defaultMethod() const;

        private:
            const wstring mExtension;
            const BitCompressionMethod mDefaultMethod;
            const FeaturesSet mFeatures;
    };

    /**
     * @brief The namespace BitFormat contains a set of archive formats usable with bit7z classes
     */
    namespace BitFormat {

#ifdef BIT7Z_AUTO_FORMAT
        extern const BitInFormat Auto;      ///< Automatic Format Detection (available only when compiling bit7z using the BIT7Z_AUTO_FORMAT preprocessor define)
#endif

        extern const BitInFormat Rar,       ///< RAR Archive Format
                                 Arj,       ///< ARJ Archive Format
                                 Z,         ///< Z Archive Format
                                 Lzh,       ///< LZH Archive Format
                                 Cab,       ///< CAB Archive Format
                                 Nsis,      ///< NSIS Archive Format
                                 Lzma,      ///< LZMA Archive Format
                                 Lzma86,    ///< LZMA86 Archive Format
                                 Ppmd,      ///< PPMD Archive Format
                                 COFF,      ///< COFF Archive Format
                                 Ext,       ///< EXT Archive Format
                                 VMDK,      ///< VMDK Archive Format
                                 VDI,       ///< VDI Archive Format
                                 QCow,      ///< QCOW Archive Format
                                 GPT,       ///< GPT Archive Format
                                 Rar5,      ///< RAR5 Archive Format
                                 IHex,      ///< IHEX Archive Format
                                 Hxs,       ///< HXS Archive Format
                                 TE,        ///< TE Archive Format
                                 UEFIc,     ///< UEFIc Archive Format
                                 UEFIs,     ///< UEFIs Archive Format
                                 SquashFS,  ///< SquashFS Archive Format
                                 CramFS,    ///< CramFS Archive Format
                                 APM,       ///< APM Archive Format
                                 Mslz,      ///< MSLZ Archive Format
                                 Flv,       ///< FLV Archive Format
                                 Swf,       ///< SWF Archive Format
                                 Swfc,      ///< SWFC Archive Format
                                 Ntfs,      ///< NTFS Archive Format
                                 Fat,       ///< FAT Archive Format
                                 Mbr,       ///< MBR Archive Format
                                 Vhd,       ///< VHD Archive Format
                                 Pe,        ///< PE Archive Format
                                 Elf,       ///< ELF Archive Format
                                 Macho,     ///< MACHO Archive Format
                                 Udf,       ///< UDF Archive Format
                                 Xar,       ///< XAR Archive Format
                                 Mub,       ///< MUB Archive Format
                                 Hfs,       ///< HFS Archive Format
                                 Dmg,       ///< DMG Archive Format
                                 Compound,  ///< COMPOUND Archive Format
                                 Iso,       ///< ISO Archive Format
                                 Chm,       ///< CHM Archive Format
                                 Split,     ///< SPLIT Archive Format
                                 Rpm,       ///< RPM Archive Format
                                 Deb,       ///< DEB Archive Format
                                 Cpio;      ///< CPIO Archive Format

        extern const BitInOutFormat Zip,        ///< ZIP Archive Format
                                    BZip2,      ///< BZIP2 Archive Format
                                    SevenZip,   ///< 7Z Archive Format
                                    Xz,         ///< XZ Archive Format
                                    Wim,        ///< WIM Archive Format
                                    Tar,        ///< TAR Archive Format
                                    GZip;       ///< GZIP Archive Format
    }

#ifdef BIT7Z_AUTO_FORMAT
#define DEFAULT_FORMAT = BitFormat::Auto
#else
#define DEFAULT_FORMAT
#endif

}
#endif // BITFORMAT_HPP
