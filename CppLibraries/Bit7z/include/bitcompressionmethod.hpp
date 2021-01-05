#ifndef BITCOMPRESSIONMETHOD_HPP
#define BITCOMPRESSIONMETHOD_HPP

namespace bit7z {
    enum class BitCompressionMethod {
        Copy,
        Deflate,
        Deflate64,
        BZip2,
        Lzma,
        Lzma2,
        Ppmd
    };
}

#endif // BITCOMPRESSIONMETHOD_HPP
