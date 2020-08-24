const dataprovider = require("../data/dataprovider");
const register_controller = require("../endpoint_controllers/app_post_register_controller");
const padding = require("../data/scenario_data/app_register_padding_data");
const env = require("../../util/env_config");

async function register_app() {
  return await register_controller(env.REGISTER, padding);
}

module.exports = register_app;