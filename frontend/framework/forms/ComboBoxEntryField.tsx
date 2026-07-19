import { Autocomplete, Box, TextField, Typography } from "@mui/material";
import { type JSX, useEffect, useRef, useState } from "react";

/**
 * Interface representing a Combo Box option.
 */
interface ComboBoxOption<T> {
  readonly label: string;
  readonly secondaryLabel?: string | null;
  readonly value: T | null;
}

/**
 * Props for the ComboBoxEntryField component.
 */
interface ComboBoxEntryFieldProps<T> {
  readonly label: string;
  readonly options: ComboBoxOption<T>[];
  readonly value: ComboBoxOption<T> | null;
  readonly setValue?: ((newValue: ComboBoxOption<T> | null) => void) | null;
  readonly errorMessage?: string | null;
  readonly autoFocus?: boolean;
  readonly size?: "small" | "medium";
  readonly isOptionEqualToValue?: (
    option: ComboBoxOption<T>,
    value: ComboBoxOption<T>,
  ) => boolean;
}

/**
 * Compares two ComboBoxOption values for equality based on their value property.
 */
const compareOptionValues = function <T>(
  option: ComboBoxOption<T>,
  selectedValue: ComboBoxOption<T>,
): boolean {
  return Object.is(option.value, selectedValue.value);
};

/**
 * Component the presents the user with an entry field where they can enter string values.
 */
const ComboBoxEntryField = function <T>({
  label,
  options,
  value,
  setValue = null,
  errorMessage = null,
  autoFocus = false,
  size = "medium",
  isOptionEqualToValue = compareOptionValues,
}: ComboBoxEntryFieldProps<T>): JSX.Element {
  const hint = useRef("");
  const justSelected = useRef(false);
  const [inputValue, setInputValue] = useState(value?.label ?? "");

  useEffect(() => {
    setInputValue(value?.label ?? "");
  }, [value]);

  return (
    <Autocomplete
      className="combo-box-entry-field"
      clearOnBlur
      options={options}
      inputValue={inputValue}
      value={value}
      readOnly={setValue === null}
      isOptionEqualToValue={isOptionEqualToValue}
      renderOption={(props, option) => (
        <Box component="li" {...props}>
          <Box sx={{ minWidth: 0 }}>
            <Typography>{option.label}</Typography>
            {typeof option.secondaryLabel === "string" ? (
              <Typography variant="body2" color="text.secondary">
                {option.secondaryLabel}
              </Typography>
            ) : null}
          </Box>
        </Box>
      )}
      renderInput={(params) => (
        <Box sx={{ position: "relative" }}>
          <Typography
            sx={{
              position: "absolute",
              opacity: 0.5,
              left: 14,
              top: 16.5,
              overflow: "hidden",
              whiteSpace: "nowrap",
              width: "calc(100% - 75px)",
            }}
          >
            {hint.current}
          </Typography>
          <TextField
            {...params}
            label={label}
            size={size}
            error={errorMessage !== null}
            autoFocus={autoFocus}
            helperText={errorMessage ?? null}
            slotProps={{
              input: {
                ...params.InputProps,
              },
            }}
          />
        </Box>
      )}
      onChange={(_, newValue) => {
        justSelected.current = true;
        setInputValue(newValue?.label ?? "");
        setValue?.(newValue);
      }}
      onInputChange={(_, newInputValue, reason) => {
        if (reason !== "input") {
          return;
        }

        setInputValue(newInputValue);
        const normalizedInput = newInputValue.toLocaleLowerCase();
        const matchingOption = options.find((option) =>
          option.label.toLocaleLowerCase().startsWith(normalizedInput),
        );
        hint.current =
          newInputValue && matchingOption ? matchingOption.label : "";
      }}
      onKeyDown={(event) => {
        if (event.key === "Tab") {
          if (hint.current) {
            const matchingOption = options.find(
              (option) => option.label === hint.current,
            );
            if (matchingOption) {
              setInputValue(matchingOption.label);
              setValue?.(matchingOption);
            }
          }
        }
      }}
      onClose={() => {
        hint.current = "";
        if (justSelected.current) {
          justSelected.current = false;
        } else {
          setInputValue(value?.label ?? "");
        }
      }}
    />
  );
};

export { type ComboBoxOption, ComboBoxEntryField };
