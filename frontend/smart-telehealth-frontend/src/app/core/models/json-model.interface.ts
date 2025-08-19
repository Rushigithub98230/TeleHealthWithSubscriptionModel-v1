export interface JsonModel<T = any> {
  data: T;
  message?: string;
  Message?: string; // Backend uses both formats
  statusCode: number;
  StatusCode?: number; // Backend uses both formats
  meta?: {
    totalRecords: number;
    pageSize: number;
    currentPage: number;
    totalPages: number;
    defaultPageSize: number;
  };
  errors?: string[];
  isSuccess?: boolean;
  timestamp?: string;
}

export interface Meta {
  totalRecords: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  defaultPageSize: number;
}
