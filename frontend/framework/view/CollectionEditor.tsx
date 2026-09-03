import { Box, Stack } from "@mui/material";
import { Fragment, type JSX, type ReactNode, useEffect, useState } from "react";
import AddCollectionItemButton from "@/framework/view/AddCollectionItemButton";
import CollectionItemDeleteButton from "@/framework/view/CollectionItemDeleteButton";

interface CollectionItemControls {
  readonly autoFocus: boolean;
  readonly onRemove: () => void;
  readonly deleteButton: ReactNode;
}

interface CollectionEditorProps<T> {
  readonly items: T[];
  readonly setItems?: ((items: T[]) => void) | null;
  readonly createItem?: (() => T) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: ((item: T, index: number) => void) | null;
  readonly renderDeleteButton?:
    ((onRemove: () => void, disabled: boolean) => ReactNode) | null;
  readonly addLabel: string;
  readonly renderItem: (
    item: T,
    index: number,
    controls: CollectionItemControls,
  ) => ReactNode;
  readonly canDeleteItem?:
    ((item: T, index: number, items: T[]) => boolean) | null;
  readonly readOnly?: boolean;
  readonly showAddButton?: boolean;
  readonly showDeleteButton?: boolean;
  readonly getItemKey?: ((item: T, index: number) => string | number) | null;
  readonly spacing?: number;
  readonly itemContainerSx?: Record<string, unknown>;
}

/**
 * Provides common add, delete, and newly-added-item focus behavior for an
 * editable collection while leaving item layout to the caller.
 */
const CollectionEditor = function <T>({
  items,
  setItems = null,
  createItem = null,
  onAdd = null,
  onRemove = null,
  renderDeleteButton = null,
  addLabel,
  renderItem,
  canDeleteItem = null,
  readOnly = false,
  showAddButton = true,
  showDeleteButton = true,
  getItemKey = null,
  spacing = 1.5,
  itemContainerSx,
}: CollectionEditorProps<T>): JSX.Element {
  const [autoFocusItemIndex, setAutoFocusItemIndex] = useState<number | null>(
    null,
  );
  useEffect(() => {
    if (autoFocusItemIndex !== null) {
      setAutoFocusItemIndex(null);
    }
  }, [autoFocusItemIndex]);

  const renderedItems = items.map((item, index): ReactNode => {
    const deleteDisabled = canDeleteItem?.(item, index, items) === false;
    const removeItem = (): void => {
      if (readOnly || deleteDisabled) {
        return;
      }
      setAutoFocusItemIndex(null);
      if (onRemove !== null) {
        onRemove(item, index);
        return;
      }
      setItems?.(items.filter((_, itemIndex) => itemIndex !== index));
    };

    return (
      <Fragment key={getItemKey?.(item, index) ?? index}>
        {renderItem(item, index, {
          autoFocus: index === autoFocusItemIndex,
          onRemove: removeItem,
          deleteButton:
            readOnly || !showDeleteButton
              ? null
              : (renderDeleteButton?.(removeItem, deleteDisabled) ?? (
                  <CollectionItemDeleteButton
                    disabled={deleteDisabled}
                    onClick={removeItem}
                  />
                )),
        })}
      </Fragment>
    );
  });

  return (
    <Stack spacing={spacing}>
      {itemContainerSx === undefined ? (
        renderedItems
      ) : (
        <Box sx={itemContainerSx}>{renderedItems}</Box>
      )}
      {readOnly || !showAddButton ? null : (
        <AddCollectionItemButton
          label={addLabel}
          onClick={() => {
            setAutoFocusItemIndex(items.length);
            if (onAdd !== null) {
              onAdd();
              return;
            }
            if (setItems !== null && createItem !== null) {
              setItems([...items, createItem()]);
            }
          }}
        />
      )}
    </Stack>
  );
};

export type { CollectionItemControls };
export default CollectionEditor;
