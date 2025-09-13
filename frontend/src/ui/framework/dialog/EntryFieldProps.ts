import type DialogMode from "@core/fieldValues/DialogMode";

/**
 * Interface represeting common properties shared by all EntryFields.
 * @param {DialogMode} dialogMode - Current mode of the parent dialog.
 * @param {string} label - Label for this EntryField.
 * @param {Function | null} isReadOnly - Callback to determine if this field should be read-only.
 */
interface BaseEntryFieldProps {
  dialogMode: DialogMode;
  label: string;
  isReadOnly?: (() => boolean) | null;
}

/**
 * Props to an EntryField component.
 * @param {Function} setValue - Callback to update the value in this EntryField.
 * @param {T} value - Current value for this EntryField.
 */
interface EntryFieldProps<T> extends BaseEntryFieldProps {
  setValue: (newValue: T) => void;
  value: T;
}

/**
 * Props to a NullableEntryField component.
 * @param {Function} setValue - Callback to update the value in this EntryField.
 * @param {T | null} value - Current value for this EntryField, or null if not set.
 */
interface NullableEntryFieldProps<T> extends BaseEntryFieldProps {
  setValue: (newValue: T | null) => void;
  value: T | null;
}

export type { EntryFieldProps, NullableEntryFieldProps };
