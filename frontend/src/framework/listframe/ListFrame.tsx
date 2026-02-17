import "@framework/listframe/ListFrame.css";
import {
  Box,
  Paper,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import type { JSX } from "react";

/** Height of each row in the list frame. */
const listFrameRowHeight = 50;

/** Minimum number of rows to display in the list frame. */
const rowsPerPage = 10;

/** Default width for a column in the list frame. */
const defaultColumnWidth = 100;

/**
 * Props for the ListFrame component.
 */
interface ListFrameProps<T> {
  readonly name: string;
  readonly columns: ColumnDefinition<T>[];
  readonly getId: (item: T) => string;
  readonly data: T[] | null;
  readonly isLoading: boolean;
  readonly isError: boolean;
  readonly children: React.ReactNode;
}

/**
 * Component that presents a generic list frame with a table structure.
 * @template T - Type of the data items to be displayed in the list frame.
 * @param props - Props for the ListFrame component.
 * @returns JSX element representing the ListFrame.
 */
const ListFrame = function <T>({
  name,
  columns,
  getId,
  data,
  isLoading,
  isError,
  children,
}: ListFrameProps<T>): JSX.Element {
  return (
    <Box>
      <Typography variant="h6" sx={{ paddingBottom: "25px" }}>
        {name}
      </Typography>
      <Paper
        sx={{
          width: "100%",
        }}
      >
        <TableContainer>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                {columns.map((column) => (
                  <TableCell
                    key={column.name}
                    align={column.alignment ?? "left"}
                    sx={{
                      maxWidth: column.maxWidth ?? defaultColumnWidth,
                      backgroundColor: "primary.main",
                      color: "white",
                    }}
                  >
                    {column.headerContent}
                  </TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.map((item) => (
                <TableRow hover tabIndex={-1} key={getId(item)}>
                  {columns.map((column) => (
                    <TableCell
                      className="list-frame-table-cell"
                      key={`${getId(item)}-${column.name}`}
                      align={column.alignment ?? "left"}
                      sx={{
                        paddingTop: "8px",
                        paddingBottom: "8px",
                      }}
                    >
                      {column.getBodyContent(item)}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
              {(data?.length ?? 0) < rowsPerPage &&
                Array(rowsPerPage - (data?.length ?? 0))
                  .fill(null)
                  .map((_, index) => (
                    <TableRow
                      style={{ height: listFrameRowHeight }}
                      key={index}
                    >
                      {Array(columns.length)
                        .fill(null)
                        .map((__, cellIndex) => (
                          <TableCell
                            className="list-frame-table-cell"
                            key={`skeleton-${index}-${cellIndex}`}
                          >
                            {isLoading ? (
                              <Skeleton />
                            ) : isError ? (
                              <Skeleton animation={false} />
                            ) : null}
                          </TableCell>
                        ))}
                    </TableRow>
                  ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
      {children}
    </Box>
  );
};

export default ListFrame;
