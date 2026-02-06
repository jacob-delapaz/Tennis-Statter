import { defineStore } from "pinia";

export type MatchMode = "Singles" | "Doubles";

export const useMatchStore = defineStore("match", {
  state: () => ({
    mode: null as MatchMode | null,
    matchId: null as string | null,
  }),
  actions: {
    setMatch(mode: MatchMode, matchId: string) {
      this.mode = mode;
      this.matchId = matchId;
    },
    clear() {
      this.mode = null;
      this.matchId = null;
    },
  },
});
