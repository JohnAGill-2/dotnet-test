import { createRouter, createWebHistory } from "vue-router";
import PlayersList from "./views/PlayersList.vue";
import PlayerDetail from "./views/PlayerDetail.vue";
import RecommendationOutput from "./views/RecommendationOutput.vue";
import LoginView from "./views/LoginView.vue";
import { canAccessPlayer, getSession, isAdmin, isAuthenticated } from "./auth";

const routes = [
  {
    path: "/",
    name: "PlayersList",
    component: PlayersList,
  },
  {
    path: "/login",
    name: "Login",
    component: LoginView,
  },
  {
    path: "/players/:playerId",
    name: "PlayerDetail",
    component: PlayerDetail,
  },
  {
    path: "/recommendations/:playerId",
    name: "RecommendationOutput",
    component: RecommendationOutput,
    props: true,
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to) => {
  if (to.name === "Login") {
    return true;
  }

  if (!isAuthenticated()) {
    return {
      name: "Login",
      query: { redirect: to.fullPath },
    };
  }

  const session = getSession();
  if (!session) {
    return { name: "Login" };
  }

  if (to.params.playerId && !canAccessPlayer(String(to.params.playerId))) {
    if (isAdmin()) {
      return true;
    }

    return {
      name: "PlayerDetail",
      params: { playerId: session.playerId },
    };
  }

  return true;
});

export default router;
