import type { JSX, ReactNode } from "react";
import { ToggleButton, ToggleButtonGroup, Tooltip } from "@mui/material";

/**
 * An option displayed by the ToggleButtonSelector component.
 */
interface ToggleButtonSelectorOption<TValue extends string> {
  readonly value: TValue;
  readonly label: ReactNode;
  readonly disabled?: boolean;
  readonly disabledReason?: string;
}

/**
 * Props for the ToggleButtonSelector component.
 */
interface ToggleButtonSelectorProps<TValue extends string> {
  readonly value: TValue;
  readonly options: readonly ToggleButtonSelectorOption<TValue>[];
  readonly onChange: (value: TValue) => void;
}

/**
 * Displays a required, exclusive selection as a group of toggle buttons.
 */
const ToggleButtonSelector = function <TValue extends string>({
  value,
  options,
  onChange,
}: ToggleButtonSelectorProps<TValue>): JSX.Element {
  return (
    <ToggleButtonGroup
      value={value}
      exclusive
      size="small"
      onChange={(_, nextValue: TValue | null) => {
        if (nextValue !== null) {
          onChange(nextValue);
        }
      }}
      sx={{ flexShrink: 0, flexWrap: "wrap" }}
    >
      {options.map((option) => {
        const button = (
          <ToggleButton
            key={option.value}
            value={option.value}
            disabled={option.disabled}
          >
            {option.label}
          </ToggleButton>
        );
        // Disabled buttons do not fire pointer events, so the tooltip listens on a wrapping span instead
        return option.disabled === true &&
          typeof option.disabledReason === "string" ? (
          <Tooltip key={option.value} title={option.disabledReason}>
            <span>{button}</span>
          </Tooltip>
        ) : (
          button
        );
      })}
    </ToggleButtonGroup>
  );
};

export default ToggleButtonSelector;
export type { ToggleButtonSelectorOption };
