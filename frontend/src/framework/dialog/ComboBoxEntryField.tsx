import {
  Autocomplete,
  Box,
  CircularProgress,
  TextField,
  Typography,
} from "@mui/material";
import { type JSX, useRef, useState } from "react";
import type ApiErrorHandler from "@data/ApiErrorHandler";
import ErrorHelperText from "@framework/dialog/ErrorHelperText";

/**
 * Interface representing a Combo Box option.
 * @param label - Label for this option.
 * @param value - Value for this option.
 */
interface ComboBoxOption<T> {
  readonly label: string;
  readonly value: T | null;
}

/**
 * Props for the ComboBoxEntryField component.
 */
interface ComboBoxEntryFieldProps<T> {
  readonly label: string;
  readonly options: ComboBoxOption<T>[];
  readonly value: ComboBoxOption<T>;
  readonly setValue?: ((newValue: ComboBoxOption<T>) => void) | null;
  readonly loading?: boolean;
  readonly errorHandler?: ApiErrorHandler | null;
  readonly errorKey?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter string values.
 * @param props - Props for the ComboBoxEntryField component.
 * @returns JSX element representing the ComboBoxEntryField component.
 */
const ComboBoxEntryField = function <T>({
  label,
  options,
  value,
  setValue = null,
  loading = false,
  errorHandler = null,
  errorKey = null,
}: ComboBoxEntryFieldProps<T>): JSX.Element {
  const hint = useRef("");
  const [inputValue, setInputValue] = useState("");
  return (
    <Autocomplete
      className="combo-box-entry-field"
      disabled={setValue === null}
      disableClearable
      options={options}
      inputValue={inputValue}
      value={value}
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
          {/* @ts-expect-error: The params type from MUI is not correct, even though it comes from their examples and works at runtime. */}
          <TextField
            {...params}
            label={label}
            error={(errorHandler?.handleError(errorKey) ?? null) !== null}
            helperText={
              <ErrorHelperText
                errorHandler={errorHandler}
                errorKey={errorKey}
              />
            }
            slotProps={{
              input: {
                ...params.InputProps,
                endAdornment: (
                  <>
                    {loading ? (
                      <CircularProgress color="inherit" size={20} />
                    ) : null}
                    {params.InputProps.endAdornment}
                  </>
                ),
              },
            }}
            onChange={(event) => {
              const newValue = event.target.value;
              setInputValue(newValue);
              const matchingOption = options.find((option) =>
                option.label.startsWith(newValue),
              );

              if (newValue && matchingOption) {
                hint.current = matchingOption.label;
              } else {
                hint.current = "";
              }
            }}
          />
        </Box>
      )}
      loading={loading}
      onChange={(_, newValue) => {
        setValue?.(newValue);
      }}
      onKeyDown={(event) => {
        if (event.key === "Tab") {
          if (hint.current) {
            const matchingOption = options.find((option) =>
              option.label.startsWith(hint.current),
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
        setInputValue(value.label);
      }}
    />
  );
};

export { type ComboBoxOption, ComboBoxEntryField };
