import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
import ModeView from "../views/ModeView.vue";
import EnterPlayersView from "../views/EnterPlayersView.vue";
import ServeSelectionView from "../views/ServeSelectionView.vue";

export default createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", component: HomeView },
    { path: "/mode", component: ModeView },
    {
      path: "/enter-players/:matchType",
      name: "EnterPlayers",
      component: EnterPlayersView,
      props: true,
    },
    {
      path: "/serve-selection/:matchType",
      name: "ServeSelection",
      component: ServeSelectionView,
      props: true,
    },
  ],
});
