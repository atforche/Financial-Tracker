"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import {
  type TrendRangeMode,
  setTrendRangeMode,
} from "@/framework/routes/trendRange";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import { Button } from "@mui/material";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface LocationTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly locations: readonly { id: string; name: string }[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
}

/** Filters Location trends by range and included external parties. */
const LocationTrendsFilter = function ({
  accountingPeriods,
  locations,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
}: LocationTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater(["page"]);
  const mode: TrendRangeMode =
    searchParams.get("mode") === "accounting-period"
      ? "accounting-period"
      : "date";
  const selectedLocationIds = searchParams.getAll("locationIds");
  const startDate = searchParams.get("startDate") ?? defaultStartDate;
  const endDate = searchParams.get("endDate") ?? defaultEndDate;
  const startAccountingPeriodId =
    searchParams.get("startAccountingPeriodId") ??
    defaultAccountingPeriodId ??
    "";
  const endAccountingPeriodId =
    searchParams.get("endAccountingPeriodId") ??
    defaultAccountingPeriodId ??
    "";

  const updateLocationIds = function (ids: readonly string[]): void {
    updateParams((params) => {
      params.delete("locationIds");
      ids.forEach((id) => {
        params.append("locationIds", id);
      });
    });
  };

  return (
    <PageFilterFrame title="Location Trends">
      <ToggleButtonSelector
        value={mode}
        onChange={(nextMode) => {
          updateParams((params) => {
            setTrendRangeMode(params, nextMode, {
              defaultAccountingPeriodId,
              defaultStartDate,
              defaultEndDate,
            });
          });
        }}
        options={[
          { value: "date", label: "Dates" },
          {
            value: "accounting-period",
            label: "Accounting periods",
            disabled: defaultAccountingPeriodId === null,
          },
        ]}
      />
      {mode === "accounting-period" ? (
        <AccountingPeriodRangeFilter
          accountingPeriods={accountingPeriods}
          startValue={startAccountingPeriodId}
          endValue={endAccountingPeriodId}
          onChange={(range: AccountingPeriodRange) => {
            updateParams((params) => {
              params.set("startAccountingPeriodId", range.start);
              params.set("endAccountingPeriodId", range.end);
            });
          }}
        />
      ) : (
        <DateRangeFilter
          value={{ start: startDate, end: endDate }}
          onChange={(range: AccountingPeriodRange) => {
            updateParams((params) => {
              params.set("startDate", range.start);
              params.set("endDate", range.end);
            });
          }}
        />
      )}
      <MultiSelectAutocompleteFilter
        label="Locations"
        options={locations.map((location) => location.id)}
        value={selectedLocationIds}
        getOptionLabel={(id) =>
          locations.find((location) => location.id === id)?.name ?? id
        }
        placeholder="Select Locations"
        noOptionsText="No Locations found"
        onChange={updateLocationIds}
      />
      <Button
        variant="outlined"
        onClick={() => {
          updateParams((params) => {
            params.delete("locationIds");
            setTrendRangeMode(params, "date", {
              defaultAccountingPeriodId,
              defaultStartDate,
              defaultEndDate,
            });
          });
        }}
      >
        Reset filters
      </Button>
    </PageFilterFrame>
  );
};

export default LocationTrendsFilter;
