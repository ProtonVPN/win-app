#pragma once
#include <vector>
#include <cstdint>

namespace ipfilter
{
    class Buffer
    {
    public:
        Buffer(size_t size);

        Buffer(const uint8_t* data, size_t size);

        uint8_t* data();

        const uint8_t* data() const;

        size_t size();

        template <class T>
        T* as()
        {
            return reinterpret_cast<T *>(this->data());
        }

        template <class T>
        const T* as() const
        {
            return reinterpret_cast<const T *>(this->data());
        }

    private:
        std::vector<uint8_t> buffer;
    };
}
