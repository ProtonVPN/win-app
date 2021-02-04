#ifndef BITINPUTARCHIVE_H
#define BITINPUTARCHIVE_H

#include "../include/bitarchivehandler.hpp"
#include "../include/bitformat.hpp"
#include "../include/bitpropvariant.hpp"
#include "../include/bittypes.hpp"

#include <vector>
#include <string>
#include <cstdint>

struct IInStream;
struct IInArchive;
struct IOutArchive;
struct IArchiveExtractCallback;

namespace bit7z {
    using std::wstring;
    using std::vector;

    class ExtractCallback;

    class BitInputArchive {
        public:
            BitInputArchive( const BitArchiveHandler& handler, const wstring& in_file );

            BitInputArchive( const BitArchiveHandler& handler, const vector< byte_t >& in_buffer );

            BitInputArchive( const BitArchiveHandler& handler, std::istream& in_stream );

            virtual ~BitInputArchive();

            /**
             * @return the detected format of the file.
             */
            const BitInFormat& detectedFormat() const;

            /**
             * @brief Gets the specified archive property.
             *
             * @param property  the property to be retrieved.
             *
             * @return the current value of the archive property or an empty BitPropVariant if no value is specified.
             */
            BitPropVariant getArchiveProperty( BitProperty property ) const;

            /**
             * @brief Gets the specified property of an item in the archive.
             *
             * @param index     the index (in the archive) of the item.
             * @param property  the property to be retrieved.
             *
             * @return the current value of the item property or an empty BitPropVariant if the item has no value for
             * the property.
             */
            BitPropVariant getItemProperty( uint32_t index, BitProperty property ) const;

            /**
             * @return the number of items contained in the archive.
             */
            uint32_t itemsCount() const;

            /**
             * @param index the index of an item in the archive.
             *
             * @return true if and only if the item at index is a folder.
             */
            bool isItemFolder( uint32_t index ) const;

            /**
             * @param index the index of an item in the archive.
             *
             * @return true if and only if the item at index is encrypted.
             */
            bool isItemEncrypted( uint32_t index ) const;

        protected:
            IInArchive* openArchiveStream( const BitArchiveHandler& handler,
                                           const wstring& name,
                                           IInStream* in_stream );

            HRESULT initUpdatableArchive( IOutArchive** newArc ) const;

            void extract( const vector< uint32_t >& indices, ExtractCallback* extract_callback ) const;

            void test( ExtractCallback* extract_callback ) const;

            HRESULT close() const;

            friend class BitArchiveOpener;
            friend class BitExtractor;
            friend class BitMemExtractor;
            friend class BitStreamExtractor;
            friend class BitArchiveCreator;

        private:
            IInArchive* mInArchive;
            const BitInFormat* mDetectedFormat;
    };
}

#endif //BITINPUTARCHIVE_H
