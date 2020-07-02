#include "pch.h"
#include "filter_specification.h"

namespace ipfilter
{
    FilterSpecification::FilterSpecification()
    {
        this->flags = 0;
        this->permit();
        this->setWeight(0);
    }

    void FilterSpecification::block()
    {
        this->action.type = FWP_ACTION_BLOCK;
    }

    void FilterSpecification::permit()
    {
        this->action.type = FWP_ACTION_PERMIT;
    }

    void FilterSpecification::hard()
    {
        this->flags |= FWPM_FILTER_FLAG_CLEAR_ACTION_RIGHT;
    }

    void FilterSpecification::soft()
    {
        if ((this->flags & FWPM_FILTER_FLAG_CLEAR_ACTION_RIGHT) !=
            FWPM_FILTER_FLAG_CLEAR_ACTION_RIGHT)
        {
            return;
        }
        this->flags ^= FWPM_FILTER_FLAG_CLEAR_ACTION_RIGHT;
    }

    void FilterSpecification::persistent()
    {
        this->flags |= FWPM_FILTER_FLAG_PERSISTENT;
    }

    void FilterSpecification::callout(GUID* calloutKey)
    {
        this->action.type = FWP_ACTION_CALLOUT_TERMINATING;
        this->action.calloutKey = *calloutKey;
    }

    void FilterSpecification::setWeight(unsigned int weight)
    {
        if (weight > 15)
        {
            throw std::out_of_range("Weight is out of range");
        }

        this->weight.type = FWP_UINT8;
        this->weight.uint8 = weight;
    }

    FWPM_ACTION0 FilterSpecification::getAction() const
    {
        return this->action;
    }

    UINT32 FilterSpecification::getFlags() const
    {
        return this->flags;
    }

    void FilterSpecification::addCondition(const condition::Condition& condition)
    {
        this->conditions.push_back(condition);
    }

    std::vector<condition::Condition> FilterSpecification::getConditions() const
    {
        return this->conditions;
    }

    FWP_VALUE0 FilterSpecification::getWeight() const
    {
        return this->weight;
    }
}
