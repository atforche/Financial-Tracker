import "@/framework/listframe/ListFrame.css";
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnHeader from "@/framework/listframe/ColumnHeader";
import type { JSX } from "react";

/** Height of each row in the list frame. */
const listFrameRowHeight = 50;

/** Number of rows per page in the list frame. */
const rowsPerPage = 10;

/**
 * Props for the ListFrame component.
 */
interface ListFrameProps<T> {
  readonly columns: ColumnDefinition<T>[];
  readonly getId: (item: T) => string;
  readonly data: T[] | null;
}

/**
 * Component that presents a generic list frame with a table structure.
 */
const ListFrame = function <T>({
  columns,
  getId,
  data,
}: ListFrameProps<T>): JSX.Element {
  return (
    <Box>
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
                  <ColumnHeader key={column.name} column={column} />
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
                          />
                        ))}
                    </TableRow>
                  ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Box>
  );
};

export default ListFrame;
