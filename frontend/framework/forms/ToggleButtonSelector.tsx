import type { JSX, ReactNode } from "react";
import { ToggleButton, ToggleButtonGroup } from "@mui/material";

/**
 * An option displayed by the ToggleButtonSelector component.
 */
interface ToggleButtonSelectorOption<TValue extends string> {
  readonly value: TValue;
  readonly label: ReactNode;
  readonly disabled?: boolean;
}

/**
 * Props for the ToggleButtonSelector component.
 */
interface ToggleButtonSelectorProps<TValue extends string> {
  readonly value: TValue;
  readonly options: readonly ToggleButtonSelectorOption<TValue>[];
  readonly onChange: (value: TValue) => void;
  readonly disabled?: boolean;
}

/**
 * Displays a required, exclusive selection as a group of toggle buttons.
 */
const ToggleButtonSelector = function <TValue extends string>({
  value,
  options,
  onChange,
  disabled = false,
}: ToggleButtonSelectorProps<TValue>): JSX.Element {
  return (
    <ToggleButtonGroup
      value={value}
      exclusive
      disabled={disabled}
      size="small"
      onChange={(_, nextValue: TValue | null) => {
        if (nextValue !== null) {
          onChange(nextValue);
        }
      }}
      sx={{ flexShrink: 0, flexWrap: "wrap" }}
    >
      {options.map((option) => (
        <ToggleButton
          key={option.value}
          value={option.value}
          disabled={option.disabled}
        >
          {option.label}
        </ToggleButton>
      ))}
    </ToggleButtonGroup>
  );
};

export type { ToggleButtonSelectorOption, ToggleButtonSelectorProps };
export default ToggleButtonSelector;
