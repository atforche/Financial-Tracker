"use client";

import {
  ComboBoxEntryField,
  type ComboBoxOption,
} from "@/framework/forms/ComboBoxEntryField";
import type { Location, LocationDraft } from "@/locations/types";
import type { JSX } from "react";
import { useLocations } from "@/locations/LocationProvider";

/** Props for a Location entry field. */
interface LocationEntryFieldProps {
  readonly label: string;
  readonly locations?: readonly Location[] | undefined;
  readonly value: LocationDraft | null;
  readonly setValue: ((value: LocationDraft | null) => void) | null;
  readonly disabled?: boolean;
  readonly errorMessage?: string | null;
}

type LocationOption =
  | { readonly kind: "existing"; readonly location: Location }
  | { readonly kind: "new"; readonly name: string };

/** Selects an existing Location or explicitly drafts a new one. */
const LocationEntryField = function ({
  label,
  locations,
  value,
  setValue,
  disabled = false,
  errorMessage = null,
}: LocationEntryFieldProps): JSX.Element {
  const contextLocations = useLocations();
  const availableLocations = locations ?? contextLocations;
  const options: ComboBoxOption<LocationOption>[] = [...availableLocations]
    .sort((left, right): number => left.name.localeCompare(right.name))
    .map((location): ComboBoxOption<LocationOption> => ({
      label: location.name,
      value: { kind: "existing", location },
    }));
  const selectedLocation =
    value?.id === null
      ? null
      : (availableLocations.find((location) => location.id === value?.id) ??
        null);
  const selectedOption: ComboBoxOption<LocationOption> | null =
    value === null
      ? null
      : selectedLocation === null
        ? { label: value.name, value: { kind: "new", name: value.name } }
        : {
            label: selectedLocation.name,
            value: { kind: "existing", location: selectedLocation },
          };

  return (
    <ComboBoxEntryField<LocationOption>
      label={label}
      options={options}
      value={selectedOption}
      setValue={
        disabled || setValue === null
          ? null
          : (option): void => {
              if (option?.value?.kind === "new") {
                setValue({ id: null, name: option.value.name });
              } else if (option?.value?.kind === "existing") {
                const { location } = option.value;
                setValue({ id: location.id, name: location.name });
              } else {
                setValue(null);
              }
            }
      }
      errorMessage={errorMessage}
      createOption={(inputValue) => {
        const name = inputValue.trim();
        const exists = availableLocations.some(
          (location) =>
            location.name.localeCompare(name, undefined, {
              sensitivity: "accent",
            }) === 0,
        );
        return name === "" || exists
          ? null
          : {
              label: `Add new location "${name}"`,
              value: { kind: "new", name },
            };
      }}
      isOptionEqualToValue={(left, right) => left.label === right.label}
    />
  );
};

export default LocationEntryField;
