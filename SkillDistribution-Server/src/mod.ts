import path from "node:path";

import { DependencyContainer } from "tsyringe";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";
import { VFS } from "@spt/utils/VFS";

import { jsonc } from "jsonc";

class Mod implements IPreSptLoadMod
{
    private modConfig: any;

    public preSptLoad(container: DependencyContainer): void
    {
        const vfs = container.resolve<VFS>("VFS");
        this.modConfig = jsonc.parse(vfs.readFile(path.resolve(__dirname, "../config/config.jsonc")))

        const staticRouter = container.resolve<StaticRouterModService>("StaticRouterModService");

        staticRouter.registerStaticRouter("SkillDistributionModStaticRouter", 
            [
                {
                    url: "/skill-distribution/config",
                    action: async () =>
                    {
                        return JSON.stringify(this.modConfig);
                    }
                }
            ],
            "custom-static-skill-distribution"
        )
    }
}

export const mod = new Mod();
