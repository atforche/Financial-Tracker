import {
  Autocomplete,
  Box,
  TextField,
  Typography,
  createFilterOptions,
} from "@mui/material";
import { type JSX, useEffect, useRef, useState } from "react";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";

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
  readonly createOption?:
    ((inputValue: string) => ComboBoxOption<T> | null) | null;
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

const renderOptionLabel = function (
  label: string,
  inputValue: string,
): JSX.Element {
  const normalizedLabel = label.toLocaleLowerCase();
  const normalizedInput = inputValue.toLocaleLowerCase();
  const matchingPrefixLength =
    normalizedInput !== "" && normalizedLabel.startsWith(normalizedInput)
      ? inputValue.length
      : 0;

  return (
    <Typography component="div">
      <Box component="span" sx={{ fontWeight: 600 }}>
        {label.slice(0, matchingPrefixLength)}
      </Box>
      {label.slice(matchingPrefixLength)}
    </Typography>
  );
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
  createOption = null,
  isOptionEqualToValue = compareOptionValues,
}: ComboBoxEntryFieldProps<T>): JSX.Element {
  const justSelected = useRef(false);
  const [inputValue, setInputValue] = useState(value?.label ?? "");
  const [highlightedOption, setHighlightedOption] =
    useState<ComboBoxOption<T> | null>(null);
  const defaultFilterOptions = createFilterOptions<ComboBoxOption<T>>();

  const getFilteredOptions = function (
    sourceOptions: ComboBoxOption<T>[],
    currentInputValue: string,
  ): ComboBoxOption<T>[] {
    const filteredOptions = defaultFilterOptions(sourceOptions, {
      inputValue: currentInputValue,
      getOptionLabel: (option) => option.label,
    });
    const createdOption = createOption?.(currentInputValue) ?? null;
    return createdOption !== null &&
      !filteredOptions.some((option) => option.label === createdOption.label)
      ? [...filteredOptions, createdOption]
      : filteredOptions;
  };

  useEffect(() => {
    setInputValue(value?.label ?? "");
  }, [value]);

  if (setValue === null) {
    return <ReadOnlyField label={label} value={value?.label ?? null} />;
  }

  return (
    <Autocomplete
      className="combo-box-entry-field"
      autoHighlight
      clearOnBlur
      options={options}
      inputValue={inputValue}
      value={value}
      isOptionEqualToValue={isOptionEqualToValue}
      filterOptions={(sourceOptions, state) =>
        getFilteredOptions(sourceOptions, state.inputValue)
      }
      renderOption={(props, option, { inputValue: optionInputValue }) => (
        <Box component="li" {...props}>
          <Box sx={{ minWidth: 0 }}>
            {renderOptionLabel(option.label, optionInputValue)}
            {typeof option.secondaryLabel === "string" ? (
              <Typography variant="body2" color="text.secondary">
                {option.secondaryLabel}
              </Typography>
            ) : null}
          </Box>
        </Box>
      )}
      renderInput={(params) => (
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
      )}
      onChange={(_, newValue, reason) => {
        if (createOption !== null && reason === "blur") {
          return;
        }

        justSelected.current = true;
        setInputValue(newValue?.label ?? "");
        setValue(newValue);
      }}
      onInputChange={(_, newInputValue, reason) => {
        if (reason !== "input") {
          return;
        }

        setInputValue(newInputValue);
        setHighlightedOption(null);
      }}
      onKeyDown={(event) => {
        const optionToSelect =
          highlightedOption ??
          getFilteredOptions(options, inputValue)[0] ??
          null;
        const highlightedExistingOption =
          optionToSelect !== null &&
          options.some((option) => isOptionEqualToValue(option, optionToSelect))
            ? optionToSelect
            : null;

        if (event.key === "Tab" && highlightedExistingOption !== null) {
          // Let the browser move focus to the next control while preventing
          // Autocomplete from applying its own highlighted-option behavior.
          event.defaultMuiPrevented = true;
          justSelected.current = true;
          setInputValue(highlightedExistingOption.label);
          setValue(highlightedExistingOption);
        }
      }}
      onHighlightChange={(_, option) => {
        setHighlightedOption(option);
      }}
      onClose={() => {
        setHighlightedOption(null);
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
