#include "pch.h"
#include "buffer.h"

namespace ipfilter
{
    Buffer::Buffer(size_t size): buffer(std::vector<uint8_t>(size))
    {
    }

    Buffer::Buffer(const uint8_t* data, size_t size):
        buffer(std::vector<uint8_t>(data, data + size))
    {
    }

    const uint8_t* Buffer::data() const
    {
        return this->buffer.data();
    }

    uint8_t* Buffer::data()
    {
        return this->buffer.data();
    }

    size_t Buffer::size()
    {
        return this->buffer.size();
    }
}
