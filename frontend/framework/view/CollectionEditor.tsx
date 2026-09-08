import { Box, Collapse, IconButton, Stack, Typography } from "@mui/material";
import {
  Fragment,
  type JSX,
  type ReactNode,
  useEffect,
  useId,
  useState,
} from "react";
import AddCollectionItemButton from "@/framework/view/AddCollectionItemButton";
import CollectionItemDeleteButton from "@/framework/view/CollectionItemDeleteButton";
import ExpandMore from "@mui/icons-material/ExpandMore";
import InsetFrame from "@/framework/view/InsetFrame";

interface CollectionItemControls {
  readonly autoFocus: boolean;
  readonly onRemove: () => void;
  readonly deleteButton: ReactNode;
}

interface CollectionEditorProps<T> {
  readonly items: T[];
  readonly label: string;
  readonly setItems?: ((items: T[]) => void) | null;
  readonly createItem?: (() => T) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: ((item: T, index: number) => void) | null;
  readonly renderDeleteButton?:
    ((onRemove: () => void, disabled: boolean) => ReactNode) | null;
  readonly addLabel?: string;
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
  readonly spacing?: number;
  readonly itemContainerSx?: Record<string, unknown>;
}

/**
 * Provides common add, delete, and newly-added-item focus behavior for an
 * editable collection while leaving item layout to the caller.
 */
const CollectionEditor = function <T>({
  items,
  label,
  setItems = null,
  createItem = null,
  onAdd = null,
  onRemove = null,
  renderDeleteButton = null,
  addLabel = "",
  renderItem,
  canDeleteItem = null,
  readOnly = false,
  showAddButton = true,
  showDeleteButton = true,
  spacing = 1.5,
  itemContainerSx,
}: CollectionEditorProps<T>): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const [autoFocusItemIndex, setAutoFocusItemIndex] = useState<number | null>(
    null,
  );
  const detailsId = useId();
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
      <Fragment key={index}>
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

  const content = (
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

  if (!readOnly || items.length <= 1) {
    return content;
  }

  return (
    <InsetFrame>
      <Stack direction="row" alignItems="center" justifyContent="space-between">
        <Typography variant="subtitle1">
          {label} ({items.length})
        </Typography>
        <IconButton
          size="small"
          onClick={() => {
            setExpanded((current) => !current);
          }}
          aria-label={`${expanded ? "Collapse" : "Expand"} ${label}`}
          aria-expanded={expanded}
          aria-controls={detailsId}
          sx={{
            p: 0.5,
            transform: expanded ? "rotate(180deg)" : "rotate(0deg)",
            transition: "transform 0.3s ease-in-out",
          }}
        >
          <ExpandMore />
        </IconButton>
      </Stack>
      <Collapse id={detailsId} in={expanded} timeout="auto" unmountOnExit>
        <Box sx={{ pt: 1.5 }}>{content}</Box>
      </Collapse>
    </InsetFrame>
  );
};

export type { CollectionItemControls };
export default CollectionEditor;
