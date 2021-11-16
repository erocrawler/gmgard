
export interface RaffleConfig {
  id: number
  title: string
  eventStart: Date
  eventEnd: Date
  isActive?: boolean
  hasRaffle?: boolean
  points?: number
  raffleCost: number
  image: string
}
