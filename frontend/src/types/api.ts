/** Formato RFC 7807 — Problem Details devolvido pelo backend em todos os erros */
export interface ApiError {
  type: string
  title: string
  status: number
  detail: string
  instance: string
  traceId: string
}
